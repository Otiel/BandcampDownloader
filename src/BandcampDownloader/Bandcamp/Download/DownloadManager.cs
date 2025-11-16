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
using NLog;

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
    private readonly IImageService _imageService;
    private readonly IUserSettings _userSettings;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private bool _isInitialized;
    private IReadOnlyList<TrackFile> _downloadingFiles;
    private IReadOnlyList<Album> _albums;

    public event DownloadProgressChangedEventHandler DownloadProgressChanged;

    public DownloadManager(IFileService fileService, IPlaylistCreator playlistCreator, ITagService tagService, IResilienceService resilienceService, ITrackFileService trackFileService, IAlbumUrlRetriever albumUrlRetriever, IAlbumInfoRetriever albumInfoRetriever, IImageService imageService, ISettingsService settingsService)
    {
        _fileService = fileService;
        _playlistCreator = playlistCreator;
        _tagService = tagService;
        _resilienceService = resilienceService;
        _trackFileService = trackFileService;
        _albumUrlRetriever = albumUrlRetriever;
        _albumInfoRetriever = albumInfoRetriever;
        _imageService = imageService;
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

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _userSettings.MaxConcurrentAlbumsDownloads,
            };

            await Parallel.ForEachAsync(
                _albums,
                parallelOptions,
                async (album, ct) =>
                {
                    await DownloadAlbumAsync(album, ct);
                });
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

        // Create a directory to save track files
        try
        {
            Directory.CreateDirectory(album.Path);
        }
        catch
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs("An error occurred when creating the album folder. Make sure you have the rights to write files in the folder you chose", DownloadProgressChangedLevel.Error));
            return;
        }

        Stream inTagsArtworkStream = null;

        // Download artwork
        if ((_userSettings.SaveCoverArtInTags || _userSettings.SaveCoverArtInFolder) && album.HasArtwork)
        {
            await using var artworkStream = await DownloadCoverArtAsync(album, cancellationToken);

            // Save artwork to folder
            if (_userSettings.SaveCoverArtInFolder && artworkStream != null)
            {
                await SaveCoverArtToFolder(album, artworkStream, cancellationToken);
            }

            // Prepare artwork for tags
            if (_userSettings.SaveCoverArtInTags && artworkStream != null)
            {
                inTagsArtworkStream = await PrepareCoverArtForTags(artworkStream, cancellationToken);
            }
        }

        // Download & tag tracks
        var downloadedTracksCount = 0;

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = _userSettings.MaxConcurrentTracksDownloads,
        };

        await Parallel.ForEachAsync(
            album.Tracks,
            parallelOptions,
            async (track, ct) =>
            {
                var trackDownloaded = await DownloadAndTagTrackAsync(album, track, inTagsArtworkStream, ct);
                if (trackDownloaded)
                {
                    Interlocked.Increment(ref downloadedTracksCount);
                }
            });

        // Cleanup
        if (inTagsArtworkStream != null)
        {
            await inTagsArtworkStream.DisposeAsync();
        }

        // Create playlist file
        if (_userSettings.CreatePlaylist)
        {
            await _playlistCreator.SavePlaylistToFileAsync(album, cancellationToken);
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Saved playlist for album \"{album.Title}\"", DownloadProgressChangedLevel.IntermediateSuccess));
        }

        if (album.Tracks.Count == downloadedTracksCount)
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Successfully downloaded album \"{album.Title}\"", DownloadProgressChangedLevel.Success));
        }
        else
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Finished downloading album \"{album.Title}\". Some tracks were not downloaded", DownloadProgressChangedLevel.Success));
        }
    }

    private async Task SaveCoverArtToFolder(Album album, Stream artworkStream, CancellationToken cancellationToken)
    {
        var inFolderArtworkStream = artworkStream;
        if (_userSettings.CoverArtInFolderResize)
        {
            inFolderArtworkStream = await _imageService.ResizeImage(inFolderArtworkStream, _userSettings.CoverArtInFolderMaxSize, _userSettings.CoverArtInFolderMaxSize, cancellationToken);
        }

        if (_userSettings.CoverArtInFolderConvertToJpg)
        {
            inFolderArtworkStream = await _imageService.ConvertToJpegAsync(inFolderArtworkStream, cancellationToken);
        }

        await _fileService.SaveStreamToFileAsync(inFolderArtworkStream, album.ArtworkPath, cancellationToken);
        await inFolderArtworkStream.DisposeAsync();
    }

    private async Task<Stream> PrepareCoverArtForTags(Stream artworkStream, CancellationToken cancellationToken)
    {
        var inTagsArtworkStream = artworkStream;
        if (_userSettings.CoverArtInTagsResize)
        {
            inTagsArtworkStream = await _imageService.ResizeImage(inTagsArtworkStream, _userSettings.CoverArtInTagsMaxSize, _userSettings.CoverArtInTagsMaxSize, cancellationToken);
        }

        if (_userSettings.CoverArtInTagsConvertToJpg)
        {
            inTagsArtworkStream = await _imageService.ConvertToJpegAsync(inTagsArtworkStream, cancellationToken);
        }

        return inTagsArtworkStream;
    }

    /// <summary>
    /// Downloads and tags a track. Returns true if the track has been correctly downloaded; false otherwise.
    /// </summary>
    /// <param name="album">The album of the track to download.</param>
    /// <param name="track">The track to download.</param>
    /// <param name="artworkStream">The cover art.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    private async Task<bool> DownloadAndTagTrackAsync(Album album, Track track, Stream artworkStream, CancellationToken cancellationToken)
    {
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

                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloading track \"{track.Title}\" from url: {track.Mp3Url}", DownloadProgressChangedLevel.VerboseInfo));
                await downloadService.DownloadFileTaskAsync(track.Mp3Url, track.Path, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested(); // See https://github.com/bezzad/Downloader/issues/203
                trackDownloaded = true;
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloaded track \"{track.Title}\" from url: {track.Mp3Url}", DownloadProgressChangedLevel.VerboseInfo));
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
                if (_userSettings.ModifyTags ||
                    _userSettings.SaveCoverArtInTags && artworkStream != null)
                {
                    await _tagService.SaveTagsInTrackAsync(track, album, artworkStream, cancellationToken);
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Tags saved for track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", DownloadProgressChangedLevel.VerboseInfo));
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
    /// Downloads and returns the cover art of the specified album.
    /// Returns null if the cover art could not be downloaded in the max tries attempts.
    /// </summary>
    /// <param name="album">The album.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    private async Task<Stream> DownloadCoverArtAsync(Album album, CancellationToken cancellationToken)
    {
        Stream artworkStream = null;
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
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloading artwork from url: {album.ArtworkUrl}", DownloadProgressChangedLevel.VerboseInfo));

            try
            {
                artworkStream = await downloadService.DownloadFileTaskAsync(album.ArtworkUrl, cancellationToken);
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

        return artworkStream;
    }
}
