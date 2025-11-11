using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Audio;
using BandcampDownloader.Helpers;
using BandcampDownloader.IO;
using BandcampDownloader.Model;
using BandcampDownloader.Settings;
using Downloader;
using ImageResizer;
using NLog;
using TagLib;
using File = System.IO.File;

namespace BandcampDownloader.Bandcamp.Download;

internal interface IDownloadManager
{
    Task InitializeAsync(string inputUrls, CancellationToken cancellationToken);
    Task StartDownloadsAsync(CancellationToken cancellationToken);
    long GetTotalBytesReceived();
    long GetTotalBytesToDownload();
    int GetTotalFilesCountReceived();
    int GetTotalFilesCountToDownload();
    event DownloadProgressChangedEventHandler DownloadProgressChanged;
}

internal sealed class DownloadManager : IDownloadManager
{
    private readonly IFileService _fileService;
    private readonly IPlaylistCreator _playlistCreator;
    private readonly ITagService _tagService;
    private readonly ITrackFileService _trackFileService;
    private readonly IAlbumUrlRetriever _albumUrlRetriever;
    private readonly IAlbumInfoRetriever _albumInfoRetriever;
    private readonly IResilienceService _resilienceService;
    private readonly IUserSettings _userSettings;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private bool _isInitialized;
    private IReadOnlyList<TrackFile> _downloadingFiles;
    private IReadOnlyList<Album> _albums;

    public event DownloadProgressChangedEventHandler DownloadProgressChanged;

    public DownloadManager(IFileService fileService, IPlaylistCreator playlistCreator, ITagService tagService, IResilienceService resilienceService, ITrackFileService trackFileService, IAlbumUrlRetriever albumUrlRetriever, IAlbumInfoRetriever albumInfoRetriever, ISettingsService settingsService)
    {
        _fileService = fileService;
        _playlistCreator = playlistCreator;
        _tagService = tagService;
        _resilienceService = resilienceService;
        _trackFileService = trackFileService;
        _albumUrlRetriever = albumUrlRetriever;
        _albumInfoRetriever = albumInfoRetriever;
        _userSettings = settingsService.GetUserSettings();

        _albumUrlRetriever.DownloadProgressChanged += OnDownloadProgressChanged;
        _albumInfoRetriever.DownloadProgressChanged += OnDownloadProgressChanged;
        _trackFileService.DownloadProgressChanged += OnDownloadProgressChanged;
    }

    private void OnDownloadProgressChanged(object sender, DownloadProgressChangedArgs eventArgs)
    {
        DownloadProgressChanged?.Invoke(sender, eventArgs);
    }

    public async Task InitializeAsync(string inputUrls, CancellationToken cancellationToken)
    {
        var albumsUrls = await _albumUrlRetriever.RetrieveAlbumsUrlsAsync(inputUrls, _userSettings.DownloadArtistDiscography, cancellationToken);
        _albums = await _albumInfoRetriever.GetAlbumsAsync(albumsUrls, cancellationToken);
        _downloadingFiles = await _trackFileService.GetFilesToDownloadAsync(_albums, cancellationToken);

        _isInitialized = true;
    }

    public async Task StartDownloadsAsync(CancellationToken cancellationToken)
    {
        try
        {
            ThrowIfNotInitialized();

            if (_userSettings.DownloadOneAlbumAtATime)
            {
                // Download one album at a time
                foreach (var album in _albums)
                {
                    await DownloadAlbumAsync(album, cancellationToken);
                }
            }
            else
            {
                // Parallel download
                var albumsIndexes = Enumerable.Range(0, _albums.Count).ToArray();
                await Task.WhenAll(albumsIndexes.Select(i => DownloadAlbumAsync(_albums[i], cancellationToken)));
            }
        }
        finally
        {
            _isInitialized = false;
        }
    }

    public long GetTotalBytesReceived()
    {
        return _downloadingFiles.Sum(f => f.BytesReceived);
    }

    public long GetTotalBytesToDownload()
    {
        return _downloadingFiles.Sum(f => f.Size);
    }

    public int GetTotalFilesCountToDownload()
    {
        return _downloadingFiles.Count;
    }

    public int GetTotalFilesCountReceived()
    {
        return _downloadingFiles.Count(f => f.Downloaded);
    }

    private void ThrowIfNotInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(DownloadManager)} has not been initialized.");
        }
    }

    private async Task DownloadAlbumAsync(Album album, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Create a directory to place track files
        try
        {
            Directory.CreateDirectory(album.Path);
        }
        catch
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs("An error occured when creating the album folder. Make sure you have the rights to write files in the folder you chose", DownloadProgressChangedLevel.Error));
            return;
        }

        Picture artwork = null;

        // Download artwork
        if ((_userSettings.SaveCoverArtInTags || _userSettings.SaveCoverArtInFolder) && album.HasArtwork)
        {
            artwork = await DownloadCoverArtAsync(album, cancellationToken);
        }

        // Download & tag tracks
        var tracksDownloaded = new bool[album.Tracks.Count];
        var indexes = Enumerable.Range(0, album.Tracks.Count).ToArray();
        await Task.WhenAll(indexes.Select(async i => tracksDownloaded[i] = await DownloadAndTagTrackAsync(album, album.Tracks[i], artwork, cancellationToken)));

        // Create playlist file
        if (_userSettings.CreatePlaylist)
        {
            await _playlistCreator.SavePlaylistToFileAsync(album, cancellationToken);
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Saved playlist for album \"{album.Title}\"", DownloadProgressChangedLevel.IntermediateSuccess));
        }

        if (tracksDownloaded.All(x => x))
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Successfully downloaded album \"{album.Title}\"", DownloadProgressChangedLevel.Success));
        }
        else
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Finished downloading album \"{album.Title}\". Some tracks were not downloaded", DownloadProgressChangedLevel.Success));
        }
    }

    /// <summary>
    /// Downloads and tags a track. Returns true if the track has been correctly downloaded; false otherwise.
    /// </summary>
    /// <param name="album">The album of the track to download.</param>
    /// <param name="track">The track to download.</param>
    /// <param name="artwork">The cover art.</param>
    /// <param name="cancellationToken"></param>
    private async Task<bool> DownloadAndTagTrackAsync(Album album, Track track, Picture artwork, CancellationToken cancellationToken)
    {
        var trackMp3Url = UrlHelper.GetHttpUrlIfNeeded(track.Mp3Url);
        var tries = 0;
        var trackDownloaded = false;

        TrackFile currentFile;
        try
        {
            currentFile = _downloadingFiles.First(f => f.Url == track.Mp3Url);
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            throw;
        }

        if (File.Exists(track.Path))
        {
            var length = new FileInfo(track.Path).Length;
            if (currentFile.Size > length - currentFile.Size * _userSettings.AllowedFileSizeDifference &&
                currentFile.Size < length + currentFile.Size * _userSettings.AllowedFileSizeDifference)
            {
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Track already exists within allowed file size range: track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\" - Skipping download!", DownloadProgressChangedLevel.IntermediateSuccess));
                return false;
            }
        }

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var downloadService = new DownloadService();

            // Update progress bar when downloading
            downloadService.DownloadProgressChanged += (_, args) =>
            {
                currentFile.BytesReceived = args.ReceivedBytesSize;
            };

            // Start download
            try
            {
                if (track.Path == null)
                {
                    throw new InvalidOperationException("Track path is null");
                }

                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloading track \"{track.Title}\" from url: {trackMp3Url}", DownloadProgressChangedLevel.VerboseInfo));
                await downloadService.DownloadFileTaskAsync(trackMp3Url, track.Path, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested(); // See https://github.com/bezzad/Downloader/issues/203
                trackDownloaded = true;
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloaded track \"{track.Title}\" from url: {trackMp3Url}", DownloadProgressChangedLevel.VerboseInfo));
            }
            catch (WebException) // TODO is this still a WebException?
            {
                // Connection closed probably because no response from Bandcamp
                if (tries + 1 < _userSettings.DownloadMaxTries)
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Try {tries + 1} of {_userSettings.DownloadMaxTries}", DownloadProgressChangedLevel.Warning));
                }
                else
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Hit max retries of {_userSettings.DownloadMaxTries}", DownloadProgressChangedLevel.Error));
                }
            }

            if (trackDownloaded)
            {
                if (_userSettings.ModifyTags)
                {
                    // Tag (ID3) the file when downloaded
                    await Task.Run(() =>
                    {
                        var tagFile = TagLib.File.Create(track.Path);
                        tagFile = _tagService.UpdateArtist(tagFile, album.Artist, _userSettings.TagArtist);
                        tagFile = _tagService.UpdateAlbumArtist(tagFile, album.Artist, _userSettings.TagAlbumArtist);
                        tagFile = _tagService.UpdateAlbumTitle(tagFile, album.Title, _userSettings.TagAlbumTitle);
                        tagFile = _tagService.UpdateAlbumYear(tagFile, (uint)album.ReleaseDate.Year, _userSettings.TagYear);
                        tagFile = _tagService.UpdateTrackNumber(tagFile, (uint)track.Number, _userSettings.TagTrackNumber);
                        tagFile = _tagService.UpdateTrackTitle(tagFile, track.Title, _userSettings.TagTrackTitle);
                        tagFile = _tagService.UpdateTrackLyrics(tagFile, track.Lyrics, _userSettings.TagLyrics);
                        tagFile = _tagService.UpdateComments(tagFile, _userSettings.TagComments);
                        tagFile.Save();
                    }, cancellationToken);
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Tags saved for track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", DownloadProgressChangedLevel.VerboseInfo));
                }

                if (_userSettings.SaveCoverArtInTags && artwork != null)
                {
                    // Save cover in tags when downloaded
                    await Task.Run(() =>
                    {
                        var tagFile = TagLib.File.Create(track.Path);
                        tagFile.Tag.Pictures = new IPicture[] { artwork };
                        tagFile.Save();
                    }, cancellationToken);
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Cover art saved in tags for track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", DownloadProgressChangedLevel.VerboseInfo));
                }

                // Note the file as downloaded
                currentFile.Downloaded = true;
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloaded track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", DownloadProgressChangedLevel.IntermediateSuccess));
            }

            tries++;
            if (!trackDownloaded && tries < _userSettings.DownloadMaxTries)
            {
                await _resilienceService.WaitForCooldownAsync(tries, cancellationToken);
            }
        }
        while (!trackDownloaded && tries < _userSettings.DownloadMaxTries);

        return trackDownloaded;
    }

    /// <summary>
    /// Downloads and returns the cover art of the specified album. Depending on UserSettings, save the cover art in the album folder.
    /// </summary>
    /// <param name="album">The album.</param>
    /// <param name="cancellationToken"></param>
    private async Task<Picture> DownloadCoverArtAsync(Album album, CancellationToken cancellationToken)
    {
        Picture artworkInTags = null;

        var tries = 0;
        var artworkDownloaded = false;

        TrackFile currentFile;
        try
        {
            currentFile = _downloadingFiles.First(f => f.Url == album.ArtworkUrl);
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            throw;
        }

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var downloadService = new DownloadService();

            // Update progress bar when downloading
            downloadService.DownloadProgressChanged += (_, args) =>
            {
                currentFile.BytesReceived = args.ReceivedBytesSize;
            };

            // Start download
            var albumArtworkUrl = UrlHelper.GetHttpUrlIfNeeded(album.ArtworkUrl);
            try
            {
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloading artwork from url: {album.ArtworkUrl}", DownloadProgressChangedLevel.VerboseInfo));
                await downloadService.DownloadFileTaskAsync(albumArtworkUrl, album.ArtworkTempPath, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested(); // See https://github.com/bezzad/Downloader/issues/203
                artworkDownloaded = true;
            }
            catch (WebException) // TODO is this still a WebException?
            {
                // Connection closed probably because no response from Bandcamp
                if (tries < _userSettings.DownloadMaxTries)
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Unable to download artwork for album \"{album.Title}\". Try {tries + 1} of {_userSettings.DownloadMaxTries}", DownloadProgressChangedLevel.Warning));
                }
                else
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Unable to download artwork for album \"{album.Title}\". Hit max retries of {_userSettings.DownloadMaxTries}", DownloadProgressChangedLevel.Error));
                }
            }

            if (artworkDownloaded)
            {
                // Convert/resize artwork to be saved in album folder
                if (_userSettings.SaveCoverArtInFolder && (_userSettings.CoverArtInFolderConvertToJpg || _userSettings.CoverArtInFolderResize))
                {
                    var settings = new ResizeSettings();
                    if (_userSettings.CoverArtInFolderConvertToJpg)
                    {
                        settings.Format = "jpg";
                        settings.Quality = 90;
                    }

                    if (_userSettings.CoverArtInFolderResize)
                    {
                        settings.MaxHeight = _userSettings.CoverArtInFolderMaxSize;
                        settings.MaxWidth = _userSettings.CoverArtInFolderMaxSize;
                    }

                    await Task.Run(() =>
                    {
                        ImageBuilder.Current.Build(album.ArtworkTempPath, album.ArtworkPath, settings); // Save it to the album folder
                    }, cancellationToken);
                }
                else if (_userSettings.SaveCoverArtInFolder)
                {
                    await _fileService.CopyFileAsync(album.ArtworkTempPath, album.ArtworkPath, cancellationToken);
                }

                // Convert/resize artwork to be saved in tags
                if (_userSettings.SaveCoverArtInTags && (_userSettings.CoverArtInTagsConvertToJpg || _userSettings.CoverArtInTagsResize))
                {
                    var settings = new ResizeSettings();
                    if (_userSettings.CoverArtInTagsConvertToJpg)
                    {
                        settings.Format = "jpg";
                        settings.Quality = 90;
                    }

                    if (_userSettings.CoverArtInTagsResize)
                    {
                        settings.MaxHeight = _userSettings.CoverArtInTagsMaxSize;
                        settings.MaxWidth = _userSettings.CoverArtInTagsMaxSize;
                    }

                    await Task.Run(() =>
                    {
                        ImageBuilder.Current.Build(album.ArtworkTempPath, album.ArtworkTempPath, settings); // Save it to %Temp%
                    }, cancellationToken);
                }

                artworkInTags = new Picture(album.ArtworkTempPath)
                {
                    Description = "Picture",
                };

                try
                {
                    File.Delete(album.ArtworkTempPath);
                }
                catch
                {
                    // Could not delete the file. Never mind, it's in %Temp% folder...
                }

                // Note the file as downloaded
                currentFile.Downloaded = true;
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloaded artwork for album \"{album.Title}\"", DownloadProgressChangedLevel.IntermediateSuccess));
            }

            tries++;
            if (!artworkDownloaded && tries < _userSettings.DownloadMaxTries)
            {
                await _resilienceService.WaitForCooldownAsync(tries, cancellationToken);
            }
        }
        while (!artworkDownloaded && tries < _userSettings.DownloadMaxTries);

        return artworkInTags;
    }
}
