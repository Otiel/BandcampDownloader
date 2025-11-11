using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Helpers;
using BandcampDownloader.IO;
using BandcampDownloader.Logging;
using BandcampDownloader.Model;
using BandcampDownloader.Net;
using BandcampDownloader.Playlist;
using BandcampDownloader.Settings;
using Downloader;
using ImageResizer;
using NLog;
using TagLib;
using File = System.IO.File;

namespace BandcampDownloader.Core;

internal interface IDownloadManager
{
    /// <summary>
    /// Fetch albums data from the specified URLs.
    /// </summary>
    Task FetchUrlsAsync(string urls, CancellationToken cancellationToken);

    /// <summary>
    /// Starts downloads.
    /// </summary>
    Task StartDownloadsAsync(CancellationToken cancellationToken);

    event DownloadManager.LogAddedEventHandler LogAdded;
    long GetTotalBytesReceived();
    long GetTotalBytesToDownload();
    long GetTotalFilesCountToDownload();
    double GetTotalFilesCountReceived();
}

internal sealed class DownloadManager : IDownloadManager
{
    private readonly IBandcampExtractionService _bandcampExtractionService;
    private readonly IFileService _fileService;
    private readonly IHttpService _httpService;
    private readonly IPlaylistCreator _playlistCreator;
    private readonly ITagService _tagService;
    private readonly IUserSettings _userSettings;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The albums to download.
    /// </summary>
    private List<Album> _albums;

    private readonly Lock _downloadingFilesLock = new();
    private List<TrackFile> _downloadingFiles;

    public delegate void LogAddedEventHandler(object sender, LogArgs eventArgs);

    public event LogAddedEventHandler LogAdded;

    public DownloadManager(IBandcampExtractionService bandcampExtractionService, IFileService fileService, IHttpService httpService, IPlaylistCreator playlistCreator, ISettingsService settingsService, ITagService tagService)
    {
        _bandcampExtractionService = bandcampExtractionService;
        _fileService = fileService;
        _httpService = httpService;
        _playlistCreator = playlistCreator;
        _tagService = tagService;
        _userSettings = settingsService.GetUserSettings();
    }

    public async Task FetchUrlsAsync(string urls, CancellationToken cancellationToken)
    {
        var sanitizedUrls = urls.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries).ToList();
        sanitizedUrls = sanitizedUrls.Distinct().Select(o => o.Trim()).ToList();

        // Get URLs of albums to download
        if (_userSettings.DownloadArtistDiscography)
        {
            sanitizedUrls = await GetArtistDiscographyAsync(sanitizedUrls, cancellationToken);
        }

        // Get info on albums
        _albums = await GetAlbumsAsync(sanitizedUrls, cancellationToken);

        // Save the files to download and get their size
        var filesToDownload = await GetFilesToDownloadAsync(_albums, cancellationToken);
        lock (_downloadingFilesLock)
        {
            _downloadingFiles = filesToDownload;
        }
    }

    public async Task StartDownloadsAsync(CancellationToken cancellationToken)
    {
        if (_albums == null)
        {
            throw new Exception("Must call FetchUrls before calling StartDownloadsAsync");
        }

        // Start downloading albums
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

    public long GetTotalBytesReceived()
    {
        lock (_downloadingFilesLock)
        {
            return _downloadingFiles.Sum(f => f.BytesReceived);
        }
    }

    public long GetTotalBytesToDownload()
    {
        lock (_downloadingFilesLock)
        {
            return _downloadingFiles.Sum(f => f.Size);
        }
    }

    public long GetTotalFilesCountToDownload()
    {
        lock (_downloadingFilesLock)
        {
            return _downloadingFiles.Count;
        }
    }

    public double GetTotalFilesCountReceived()
    {
        lock (_downloadingFilesLock)
        {
            return _downloadingFiles.Count(f => f.Downloaded);
        }
    }

    /// <summary>
    /// Downloads an album.
    /// </summary>
    /// <param name="album">The album to download.</param>
    /// <param name="cancellationToken"></param>
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
            LogAdded?.Invoke(this, new LogArgs("An error occured when creating the album folder. Make sure you have the rights to write files in the folder you chose", LogType.Error));
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
            LogAdded?.Invoke(this, new LogArgs($"Saved playlist for album \"{album.Title}\"", LogType.IntermediateSuccess));
        }

        if (tracksDownloaded.All(x => x))
        {
            LogAdded?.Invoke(this, new LogArgs($"Successfully downloaded album \"{album.Title}\"", LogType.Success));
        }
        else
        {
            LogAdded?.Invoke(this, new LogArgs($"Finished downloading album \"{album.Title}\". Some tracks were not downloaded", LogType.Success));
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
        lock (_downloadingFilesLock)
        {
            currentFile = _downloadingFiles.First(f => f.Url == track.Mp3Url);
        }

        if (File.Exists(track.Path))
        {
            var length = new FileInfo(track.Path).Length;
            if (currentFile.Size > length - currentFile.Size * _userSettings.AllowedFileSizeDifference &&
                currentFile.Size < length + currentFile.Size * _userSettings.AllowedFileSizeDifference)
            {
                LogAdded?.Invoke(this, new LogArgs($"Track already exists within allowed file size range: track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\" - Skipping download!", LogType.IntermediateSuccess));
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

                LogAdded?.Invoke(this, new LogArgs($"Downloading track \"{track.Title}\" from url: {trackMp3Url}", LogType.VerboseInfo));
                await downloadService.DownloadFileTaskAsync(trackMp3Url, track.Path, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested(); // See https://github.com/bezzad/Downloader/issues/203
                trackDownloaded = true;
                LogAdded?.Invoke(this, new LogArgs($"Downloaded track \"{track.Title}\" from url: {trackMp3Url}", LogType.VerboseInfo));
            }
            catch (WebException) // TODO is this still a WebException?
            {
                // Connection closed probably because no response from Bandcamp
                if (tries + 1 < _userSettings.DownloadMaxTries)
                {
                    LogAdded?.Invoke(this, new LogArgs($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Try {tries + 1} of {_userSettings.DownloadMaxTries}", LogType.Warning));
                }
                else
                {
                    LogAdded?.Invoke(this, new LogArgs($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Hit max retries of {_userSettings.DownloadMaxTries}", LogType.Error));
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
                    LogAdded?.Invoke(this, new LogArgs($"Tags saved for track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", LogType.VerboseInfo));
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
                    LogAdded?.Invoke(this, new LogArgs($"Cover art saved in tags for track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", LogType.VerboseInfo));
                }

                // Note the file as downloaded
                currentFile.Downloaded = true;
                LogAdded?.Invoke(this, new LogArgs($"Downloaded track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", LogType.IntermediateSuccess));
            }

            tries++;
            if (!trackDownloaded && tries < _userSettings.DownloadMaxTries)
            {
                await WaitForCooldownAsync(tries, cancellationToken);
            }
        }
        while (!trackDownloaded && tries < _userSettings.DownloadMaxTries);

        return trackDownloaded;
    }

    /// <summary>
    /// Downloads and returns the cover art of the specified album. Depending on UserSettings, save the cover art in
    /// the album folder.
    /// </summary>
    /// <param name="album">The album.</param>
    /// <param name="cancellationToken"></param>
    private async Task<Picture> DownloadCoverArtAsync(Album album, CancellationToken cancellationToken)
    {
        Picture artworkInTags = null;

        var tries = 0;
        var artworkDownloaded = false;
        TrackFile currentFile;
        lock (_downloadingFilesLock)
        {
            currentFile = _downloadingFiles.First(f => f.Url == album.ArtworkUrl);
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
                LogAdded?.Invoke(this, new LogArgs($"Downloading artwork from url: {album.ArtworkUrl}", LogType.VerboseInfo));
                await downloadService.DownloadFileTaskAsync(albumArtworkUrl, album.ArtworkTempPath, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested(); // See https://github.com/bezzad/Downloader/issues/203
                artworkDownloaded = true;
            }
            catch (WebException) // TODO is this still a WebException?
            {
                // Connection closed probably because no response from Bandcamp
                if (tries < _userSettings.DownloadMaxTries)
                {
                    LogAdded?.Invoke(this, new LogArgs($"Unable to download artwork for album \"{album.Title}\". Try {tries + 1} of {_userSettings.DownloadMaxTries}", LogType.Warning));
                }
                else
                {
                    LogAdded?.Invoke(this, new LogArgs($"Unable to download artwork for album \"{album.Title}\". Hit max retries of {_userSettings.DownloadMaxTries}", LogType.Error));
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
                    // Could not delete the file. Nevermind, it's in %Temp% folder...
                }

                // Note the file as downloaded
                currentFile.Downloaded = true;
                LogAdded?.Invoke(this, new LogArgs($"Downloaded artwork for album \"{album.Title}\"", LogType.IntermediateSuccess));
            }

            tries++;
            if (!artworkDownloaded && tries < _userSettings.DownloadMaxTries)
            {
                await WaitForCooldownAsync(tries, cancellationToken);
            }
        }
        while (!artworkDownloaded && tries < _userSettings.DownloadMaxTries);

        return artworkInTags;
    }

    /// <summary>
    /// Returns the albums located at the specified URLs.
    /// </summary>
    /// <param name="urls">The URLs.</param>
    /// <param name="cancellationToken"></param>
    private async Task<List<Album>> GetAlbumsAsync(List<string> urls, CancellationToken cancellationToken)
    {
        var albums = new List<Album>();

        foreach (var url in urls.Select(o => UrlHelper.GetHttpUrlIfNeeded(o)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogAdded?.Invoke(this, new LogArgs($"Retrieving album data for {url}", LogType.Info));

            // Retrieve URL HTML source code
            string htmlCode;
            try
            {
                LogAdded?.Invoke(this, new LogArgs($"Downloading album info from url: {url}", LogType.VerboseInfo));
                var httpClient = _httpService.CreateHttpClient();
                htmlCode = await httpClient.GetStringAsync(url, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(ex, $"Error downloading album info from url: {url}");
                LogAdded?.Invoke(this, new LogArgs($"Could not retrieve data for {url}", LogType.Error));
                continue;
            }

            // Get info on album
            try
            {
                var album = _bandcampExtractionService.GetAlbum(htmlCode, cancellationToken);

                if (album.Tracks.Count > 0)
                {
                    albums.Add(album);
                }
                else
                {
                    LogAdded?.Invoke(this, new LogArgs($"No tracks found for {url}, album will not be downloaded", LogType.Warning));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(ex, $"Could not retrieve album info for {url}");
                LogAdded?.Invoke(this, new LogArgs($"Could not retrieve album info for {url}", LogType.Error));
            }
        }

        return albums;
    }

    /// <summary>
    /// Returns the artists discography from any URL (artist, album, track).
    /// </summary>
    /// <param name="urls">The URLs.</param>
    /// <param name="cancellationToken"></param>
    private async Task<List<string>> GetArtistDiscographyAsync(List<string> urls, CancellationToken cancellationToken)
    {
        var albumsUrls = new List<string>();

        foreach (var url in urls.Select(o => UrlHelper.GetHttpUrlIfNeeded(o)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogAdded?.Invoke(this, new LogArgs($"Retrieving artist discography from {url}", LogType.Info));

            // Get artist "music" bandcamp page (http://artist.bandcamp.com/music)
            var regex = new Regex("https?://[^/]*");
            var artistPage = regex.Match(url).ToString();
            var artistMusicPage = UrlHelper.GetHttpUrlIfNeeded(artistPage + "/music");

            // Retrieve artist "music" page HTML source code
            string htmlCode;

            try
            {
                LogAdded?.Invoke(this, new LogArgs($"Downloading album info from url: {url}", LogType.VerboseInfo));
                var httpClient = _httpService.CreateHttpClient();
                htmlCode = await httpClient.GetStringAsync(artistMusicPage, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogAdded?.Invoke(this, new LogArgs($"Could not retrieve data for {artistMusicPage}", LogType.Error));
                continue;
            }

            var count = albumsUrls.Count;
            try
            {
                var albumsUrl = _bandcampExtractionService.GetAlbumsUrl(htmlCode, artistPage);
                albumsUrls.AddRange(albumsUrl);
            }
            catch (NoAlbumFoundException)
            {
                LogAdded?.Invoke(this, new LogArgs($"No referred album could be found on {artistMusicPage}. Try to uncheck the \"Download artist discography\" option", LogType.Error));
            }

            if (albumsUrls.Count - count == 0)
            {
                // This seems to be a one-album artist with no "music" page => URL redirects to the unique album URL
                albumsUrls.Add(url);
            }
        }

        return albumsUrls.Distinct().ToList();
    }

    /// <summary>
    /// Returns the size of the file located at the specified URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="titleForLog">The title of the file to be displayed in the log.</param>
    /// <param name="fileType">The type of the file.</param>
    /// <param name="cancellationToken"></param>
    private async Task<long> GetFileSizeAsync(string url, string titleForLog, FileType fileType, CancellationToken cancellationToken)
    {
        long size = 0;
        bool sizeRetrieved;
        var tries = 0;

        var fileTypeForLog = fileType switch
        {
            FileType.Artwork => "cover art file for album",
            FileType.Track => "MP3 file for the track",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null),
        };

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                size = await _httpService.GetFileSizeAsync(url, cancellationToken);
                sizeRetrieved = true;
                LogAdded?.Invoke(this, new LogArgs($"Retrieved the size of the {fileTypeForLog} \"{titleForLog}\"", LogType.VerboseInfo));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                sizeRetrieved = false;
                if (tries + 1 < _userSettings.DownloadMaxTries)
                {
                    LogAdded?.Invoke(this, new LogArgs($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Try {tries + 1} of {_userSettings.DownloadMaxTries}", LogType.Warning));
                }
                else
                {
                    LogAdded?.Invoke(this, new LogArgs($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Hit max retries of {_userSettings.DownloadMaxTries}. Progress update may be wrong.", LogType.Error));
                }
            }

            tries++;
            if (!sizeRetrieved && tries < _userSettings.DownloadMaxTries)
            {
                await WaitForCooldownAsync(tries, cancellationToken);
            }
        }
        while (!sizeRetrieved && tries < _userSettings.DownloadMaxTries);

        return size;
    }

    /// <summary>
    /// Returns the files to download from a list of albums.
    /// </summary>
    /// <param name="albums">The albums.</param>
    /// <param name="cancellationToken"></param>
    private async Task<List<TrackFile>> GetFilesToDownloadAsync(List<Album> albums, CancellationToken cancellationToken)
    {
        var files = new List<TrackFile>();
        foreach (var album in albums)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogAdded?.Invoke(this, new LogArgs($"Computing size for album \"{album.Title}\"...", LogType.Info));

            // Artwork
            if ((_userSettings.SaveCoverArtInTags || _userSettings.SaveCoverArtInFolder) && album.HasArtwork)
            {
                if (_userSettings.RetrieveFilesSize)
                {
                    var size = await GetFileSizeAsync(album.ArtworkUrl, album.Title, FileType.Artwork, cancellationToken);
                    files.Add(new TrackFile(album.ArtworkUrl, 0, size));
                }
                else
                {
                    files.Add(new TrackFile(album.ArtworkUrl, 0, 0));
                }
            }

            // Tracks
            if (_userSettings.RetrieveFilesSize)
            {
                var tracksIndexes = Enumerable.Range(0, album.Tracks.Count).ToArray();
                await Task.WhenAll(tracksIndexes.Select(async i =>
                {
                    var size = await GetFileSizeAsync(album.Tracks[i].Mp3Url, album.Tracks[i].Title, FileType.Track, cancellationToken);
                    files.Add(new TrackFile(album.Tracks[i].Mp3Url, 0, size));
                }));
            }
            else
            {
                foreach (var track in album.Tracks)
                {
                    files.Add(new TrackFile(track.Mp3Url, 0, 0));
                }
            }
        }

        return files;
    }

    /// <summary>
    /// Waits for a "cooldown" time, computed from the specified number of download tries.
    /// </summary>
    /// <param name="triesNumber">The times count we tried to download the same file.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task WaitForCooldownAsync(int triesNumber, CancellationToken cancellationToken)
    {
        if (_userSettings.DownloadRetryCooldown != 0)
        {
            var cooldownDelay = (int)(Math.Pow(_userSettings.DownloadRetryExponent, triesNumber) * _userSettings.DownloadRetryCooldown * 1000);
            await Task.Delay(cooldownDelay, cancellationToken);
        }
    }
}
