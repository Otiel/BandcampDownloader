﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Helpers;
using BandcampDownloader.Model;
using ImageResizer;
using TagLib;
using File = System.IO.File;

namespace BandcampDownloader.Core;

internal sealed class DownloadManager
{
    /// <summary>
    /// Object used to lock on to prevent cancellation race condition.
    /// </summary>
    private readonly object _cancellationLock = new();

    /// <summary>
    /// The URLs to download.
    /// </summary>
    private readonly string _urls;

    /// <summary>
    /// The albums to download.
    /// </summary>
    private List<Album> _albums;

    /// <summary>
    /// True if we received the order to cancel downloads; false otherwise.
    /// </summary>
    private bool _cancelDownloads;

    /// <summary>
    /// Used when downloads must be cancelled.
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// The files to download, or being downloaded, or already downloaded. Used to compute the current received bytes
    /// and the total bytes to download.
    /// </summary>
    public List<TrackFile> DownloadingFiles { get; set; }

    public delegate void LogAddedEventHandler(object sender, LogArgs eventArgs);

    /// <summary>
    /// Initializes a new instance of DownloadManager.
    /// </summary>
    /// <param name="urls">The URLs we'll download from.</param>
    public DownloadManager(string urls)
    {
        _urls = urls;

        // Increase the maximum of concurrent connections to be able to download more than 2 (which is the default
        // value) files at the same time
        ServicePointManager.DefaultConnectionLimit = 50;
    }

    /// <summary>
    /// Cancels all downloads.
    /// </summary>
    public void CancelDownloads()
    {
        lock (_cancellationLock)
        {
            _cancelDownloads = true;
            // Stop current downloads
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }
    }

    /// <summary>
    /// Fetch albums data from the URLs specified when creating this DownloadManager.
    /// </summary>
    public async Task FetchUrlsAsync()
    {
        var urls = _urls.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        urls = urls.Distinct().ToList();

        // Get URLs of albums to download
        if (App.UserSettings.DownloadArtistDiscography)
        {
            urls = await GetArtistDiscographyAsync(urls);
        }

        // Get info on albums
        _albums = await GetAlbumsAsync(urls);

        // Save the files to download and get their size
        DownloadingFiles = await GetFilesToDownloadAsync(_albums);
    }

    /// <summary>
    /// Starts downloads.
    /// </summary>
    public async Task StartDownloadsAsync()
    {
        if (_albums == null)
        {
            throw new Exception("Must call FetchUrls before calling StartDownloadsAsync");
        }

        lock (_cancellationLock)
        {
            if (_cancelDownloads)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
        }

        // Start downloading albums
        if (App.UserSettings.DownloadOneAlbumAtATime)
        {
            // Download one album at a time
            foreach (var album in _albums)
            {
                await DownloadAlbumAsync(album);
            }
        }
        else
        {
            // Parallel download
            var albumsIndexes = Enumerable.Range(0, _albums.Count).ToArray();
            await Task.WhenAll(albumsIndexes.Select(i => DownloadAlbumAsync(_albums[i])));
        }
    }

    /// <summary>
    /// Downloads an album.
    /// </summary>
    /// <param name="album">The album to download.</param>
    private async Task DownloadAlbumAsync(Album album)
    {
        if (_cancelDownloads)
        {
            // Abort
            return;
        }

        // Create directory to place track files
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
        if ((App.UserSettings.SaveCoverArtInTags || App.UserSettings.SaveCoverArtInFolder) && album.HasArtwork)
        {
            artwork = await DownloadCoverArtAsync(album);
        }

        // Download & tag tracks
        var tracksDownloaded = new bool[album.Tracks.Count];
        var indexes = Enumerable.Range(0, album.Tracks.Count).ToArray();
        await Task.WhenAll(indexes.Select(async i => tracksDownloaded[i] = await DownloadAndTagTrackAsync(album, album.Tracks[i], artwork)));

        // Create playlist file
        if (App.UserSettings.CreatePlaylist && !_cancelDownloads)
        {
            new PlaylistCreator(album).SavePlaylistToFile();
            LogAdded?.Invoke(this, new LogArgs($"Saved playlist for album \"{album.Title}\"", LogType.IntermediateSuccess));
        }

        if (!_cancelDownloads)
        {
            // Tasks have not been aborted
            if (tracksDownloaded.All(x => x))
            {
                LogAdded?.Invoke(this, new LogArgs($"Successfully downloaded album \"{album.Title}\"", LogType.Success));
            }
            else
            {
                LogAdded?.Invoke(this, new LogArgs($"Finished downloading album \"{album.Title}\". Some tracks were not downloaded", LogType.Success));
            }
        }
    }

    /// <summary>
    /// Downloads and tags a track. Returns true if the track has been correctly downloaded; false otherwise.
    /// </summary>
    /// <param name="album">The album of the track to download.</param>
    /// <param name="track">The track to download.</param>
    /// <param name="artwork">The cover art.</param>
    private async Task<bool> DownloadAndTagTrackAsync(Album album, Track track, Picture artwork)
    {
        var trackMp3Url = UrlHelper.GetHttpUrlIfNeeded(track.Mp3Url);
        LogAdded?.Invoke(this, new LogArgs($"Downloading track \"{track.Title}\" from url: {trackMp3Url}", LogType.VerboseInfo));

        var tries = 0;
        var trackDownloaded = false;
        var currentFile = DownloadingFiles.Where(f => f.Url == track.Mp3Url).First();

        if (File.Exists(track.Path))
        {
            var length = new FileInfo(track.Path).Length;
            if (currentFile.Size > length - currentFile.Size * App.UserSettings.AllowedFileSizeDifference &&
                currentFile.Size < length + currentFile.Size * App.UserSettings.AllowedFileSizeDifference)
            {
                LogAdded?.Invoke(this, new LogArgs($"Track already exists within allowed file size range: track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\" - Skipping download!", LogType.IntermediateSuccess));
                return false;
            }
        }

        do
        {
            if (_cancelDownloads)
            {
                // Abort
                return false;
            }

            using (var webClient = new WebClient())
            {
                ProxyHelper.SetProxy(webClient);

                // Update progress bar when downloading
                webClient.DownloadProgressChanged += (s, e) => { currentFile.BytesReceived = e.BytesReceived; };

                // Register current download
                _cancellationTokenSource.Token.Register(webClient.CancelAsync);

                // Start download
                try
                {
                    if (track.Path == null)
                    {
                        throw new InvalidOperationException("Track path is null");
                    }

                    LogAdded?.Invoke(this, new LogArgs($"Downloading track \"{track.Title}\" from url: {trackMp3Url}", LogType.VerboseInfo));
                    await webClient.DownloadFileTaskAsync(trackMp3Url, track.Path);
                    trackDownloaded = true;
                    LogAdded?.Invoke(this, new LogArgs($"Downloaded track \"{track.Title}\" from url: {trackMp3Url}", LogType.VerboseInfo));
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    // Downloads cancelled by the user
                    return false;
                }
                catch (TaskCanceledException)
                {
                    // Downloads cancelled by the user
                    return false;
                }
                catch (WebException)
                {
                    // Connection closed probably because no response from Bandcamp
                    if (tries + 1 < App.UserSettings.DownloadMaxTries)
                    {
                        LogAdded?.Invoke(this, new LogArgs($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Try {tries + 1} of {App.UserSettings.DownloadMaxTries}", LogType.Warning));
                    }
                    else
                    {
                        LogAdded?.Invoke(this, new LogArgs($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Hit max retries of {App.UserSettings.DownloadMaxTries}", LogType.Error));
                    }
                }

                if (trackDownloaded)
                {
                    if (App.UserSettings.ModifyTags)
                    {
                        // Tag (ID3) the file when downloaded
                        var tagFile = TagLib.File.Create(track.Path);
                        tagFile = TagHelper.UpdateArtist(tagFile, album.Artist, App.UserSettings.TagArtist);
                        tagFile = TagHelper.UpdateAlbumArtist(tagFile, album.Artist, App.UserSettings.TagAlbumArtist);
                        tagFile = TagHelper.UpdateAlbumTitle(tagFile, album.Title, App.UserSettings.TagAlbumTitle);
                        tagFile = TagHelper.UpdateAlbumYear(tagFile, (uint)album.ReleaseDate.Year, App.UserSettings.TagYear);
                        tagFile = TagHelper.UpdateTrackNumber(tagFile, (uint)track.Number, App.UserSettings.TagTrackNumber);
                        tagFile = TagHelper.UpdateTrackTitle(tagFile, track.Title, App.UserSettings.TagTrackTitle);
                        tagFile = TagHelper.UpdateTrackLyrics(tagFile, track.Lyrics, App.UserSettings.TagLyrics);
                        tagFile = TagHelper.UpdateComments(tagFile, App.UserSettings.TagComments);
                        tagFile.Save();
                        LogAdded?.Invoke(this, new LogArgs($"Tags saved for track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", LogType.VerboseInfo));
                    }

                    if (App.UserSettings.SaveCoverArtInTags && artwork != null)
                    {
                        // Save cover in tags when downloaded
                        var tagFile = TagLib.File.Create(track.Path);
                        tagFile.Tag.Pictures = new IPicture[1] { artwork };
                        tagFile.Save();
                        LogAdded?.Invoke(this, new LogArgs($"Cover art saved in tags for track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", LogType.VerboseInfo));
                    }

                    // Note the file as downloaded
                    currentFile.Downloaded = true;
                    LogAdded?.Invoke(this, new LogArgs($"Downloaded track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", LogType.IntermediateSuccess));
                }

                tries++;
                if (!trackDownloaded && tries < App.UserSettings.DownloadMaxTries)
                {
                    await WaitForCooldownAsync(tries);
                }
            }
        } while (!trackDownloaded && tries < App.UserSettings.DownloadMaxTries);

        return trackDownloaded;
    }

    /// <summary>
    /// Downloads and returns the cover art of the specified album. Depending on UserSettings, save the cover art in
    /// the album folder.
    /// </summary>
    /// <param name="album">The album.</param>
    private async Task<Picture> DownloadCoverArtAsync(Album album)
    {
        Picture artworkInTags = null;

        var tries = 0;
        var artworkDownloaded = false;
        var currentFile = DownloadingFiles.Where(f => f.Url == album.ArtworkUrl).First();

        do
        {
            if (_cancelDownloads)
            {
                // Abort
                return null;
            }

            using (var webClient = new WebClient())
            {
                ProxyHelper.SetProxy(webClient);

                // Update progress bar when downloading
                webClient.DownloadProgressChanged += (s, e) => { currentFile.BytesReceived = e.BytesReceived; };

                // Register current download
                _cancellationTokenSource.Token.Register(webClient.CancelAsync);

                // Start download
                var albumArtworkUrl = UrlHelper.GetHttpUrlIfNeeded(album.ArtworkUrl);
                try
                {
                    LogAdded?.Invoke(this, new LogArgs($"Downloading artwork from url: {album.ArtworkUrl}", LogType.VerboseInfo));
                    await webClient.DownloadFileTaskAsync(albumArtworkUrl, album.ArtworkTempPath);
                    artworkDownloaded = true;
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    // Downloads cancelled by the user
                    return null;
                }
                catch (TaskCanceledException)
                {
                    // Downloads cancelled by the user
                    return null;
                }
                catch (WebException)
                {
                    // Connection closed probably because no response from Bandcamp
                    if (tries < App.UserSettings.DownloadMaxTries)
                    {
                        LogAdded?.Invoke(this, new LogArgs($"Unable to download artwork for album \"{album.Title}\". Try {tries + 1} of {App.UserSettings.DownloadMaxTries}", LogType.Warning));
                    }
                    else
                    {
                        LogAdded?.Invoke(this, new LogArgs($"Unable to download artwork for album \"{album.Title}\". Hit max retries of {App.UserSettings.DownloadMaxTries}", LogType.Error));
                    }
                }

                if (artworkDownloaded)
                {
                    // Convert/resize artwork to be saved in album folder
                    if (App.UserSettings.SaveCoverArtInFolder && (App.UserSettings.CoverArtInFolderConvertToJpg || App.UserSettings.CoverArtInFolderResize))
                    {
                        var settings = new ResizeSettings();
                        if (App.UserSettings.CoverArtInFolderConvertToJpg)
                        {
                            settings.Format = "jpg";
                            settings.Quality = 90;
                        }

                        if (App.UserSettings.CoverArtInFolderResize)
                        {
                            settings.MaxHeight = App.UserSettings.CoverArtInFolderMaxSize;
                            settings.MaxWidth = App.UserSettings.CoverArtInFolderMaxSize;
                        }

                        await Task.Run(() =>
                        {
                            ImageBuilder.Current.Build(album.ArtworkTempPath, album.ArtworkPath, settings); // Save it to the album folder
                        });
                    }
                    else if (App.UserSettings.SaveCoverArtInFolder)
                    {
                        await FileHelper.CopyFileAsync(album.ArtworkTempPath, album.ArtworkPath);
                    }

                    // Convert/resize artwork to be saved in tags
                    if (App.UserSettings.SaveCoverArtInTags && (App.UserSettings.CoverArtInTagsConvertToJpg || App.UserSettings.CoverArtInTagsResize))
                    {
                        var settings = new ResizeSettings();
                        if (App.UserSettings.CoverArtInTagsConvertToJpg)
                        {
                            settings.Format = "jpg";
                            settings.Quality = 90;
                        }

                        if (App.UserSettings.CoverArtInTagsResize)
                        {
                            settings.MaxHeight = App.UserSettings.CoverArtInTagsMaxSize;
                            settings.MaxWidth = App.UserSettings.CoverArtInTagsMaxSize;
                        }

                        await Task.Run(() =>
                        {
                            ImageBuilder.Current.Build(album.ArtworkTempPath, album.ArtworkTempPath, settings); // Save it to %Temp%
                        });
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
                if (!artworkDownloaded && tries < App.UserSettings.DownloadMaxTries)
                {
                    await WaitForCooldownAsync(tries);
                }
            }
        } while (!artworkDownloaded && tries < App.UserSettings.DownloadMaxTries);

        return artworkInTags;
    }

    /// <summary>
    /// Returns the albums located at the specified URLs.
    /// </summary>
    /// <param name="urls">The URLs.</param>
    private async Task<List<Album>> GetAlbumsAsync(List<string> urls)
    {
        var albums = new List<Album>();

        foreach (var url in urls.Select(o => UrlHelper.GetHttpUrlIfNeeded(o)))
        {
            LogAdded?.Invoke(this, new LogArgs($"Retrieving album data for {url}", LogType.Info));

            // Retrieve URL HTML source code
            var htmlCode = "";
            using (var webClient = new WebClient { Encoding = Encoding.UTF8 })
            {
                ProxyHelper.SetProxy(webClient);

                if (_cancelDownloads)
                {
                    // Abort
                    return new List<Album>();
                }

                try
                {
                    LogAdded?.Invoke(this, new LogArgs($"Downloading album info from url: {url}", LogType.VerboseInfo));
                    htmlCode = await webClient.DownloadStringTaskAsync(url);
                }
                catch
                {
                    LogAdded?.Invoke(this, new LogArgs($"Could not retrieve data for {url}", LogType.Error));
                    continue;
                }
            }

            // Get info on album
            try
            {
                var album = BandcampHelper.GetAlbum(htmlCode);

                if (album.Tracks.Count > 0)
                {
                    albums.Add(album);
                }
                else
                {
                    LogAdded?.Invoke(this, new LogArgs($"No tracks found for {url}, album will not be downloaded", LogType.Warning));
                }
            }
            catch
            {
                LogAdded?.Invoke(this, new LogArgs($"Could not retrieve album info for {url}", LogType.Error));
            }
        }

        return albums;
    }

    /// <summary>
    /// Returns the artists discography from any URL (artist, album, track).
    /// </summary>
    /// <param name="urls">The URLs.</param>
    private async Task<List<string>> GetArtistDiscographyAsync(List<string> urls)
    {
        var albumsUrls = new List<string>();

        foreach (var url in urls.Select(o => UrlHelper.GetHttpUrlIfNeeded(o)))
        {
            LogAdded?.Invoke(this, new LogArgs($"Retrieving artist discography from {url}", LogType.Info));

            // Get artist "music" bandcamp page (http://artist.bandcamp.com/music)
            var regex = new Regex("https?://[^/]*");
            var artistPage = regex.Match(url).ToString();
            var artistMusicPage = UrlHelper.GetHttpUrlIfNeeded(artistPage + "/music");

            // Retrieve artist "music" page HTML source code
            var htmlCode = "";
            using (var webClient = new WebClient { Encoding = Encoding.UTF8 })
            {
                ProxyHelper.SetProxy(webClient);

                if (_cancelDownloads)
                {
                    // Abort
                    return new List<string>();
                }

                try
                {
                    LogAdded?.Invoke(this, new LogArgs($"Downloading album info from url: {url}", LogType.VerboseInfo));
                    htmlCode = await webClient.DownloadStringTaskAsync(artistMusicPage);
                }
                catch
                {
                    LogAdded?.Invoke(this, new LogArgs($"Could not retrieve data for {artistMusicPage}", LogType.Error));
                    continue;
                }
            }

            var count = albumsUrls.Count;
            try
            {
                albumsUrls.AddRange(BandcampHelper.GetAlbumsUrl(htmlCode, artistPage));
            }
            catch (NoAlbumFoundException)
            {
                LogAdded?.Invoke(this, new LogArgs($"No referred album could be found on {artistMusicPage}. Try to uncheck the \"Download artist discography\" option", LogType.Error));
            }

            if (albumsUrls.Count - count == 0)
            {
                // This seem to be a one-album artist with no "music" page => URL redirects to the unique album URL
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
    private async Task<long> GetFileSizeAsync(string url, string titleForLog, FileType fileType)
    {
        long size = 0;
        bool sizeRetrieved;
        var tries = 0;
        string fileTypeForLog;
        string protocolMethod;

        switch (fileType)
        {
            case FileType.Artwork:
                fileTypeForLog = "cover art file for album";
                protocolMethod = "HEAD";
                break;
            case FileType.Track:
                fileTypeForLog = "MP3 file for the track";
                // Using the HEAD method on tracks urls does not work (Error 405: Method not allowed) Surprisingly,
                // using the GET method does not seem to download the whole file, so we will use it to retrieve the
                // mp3 sizes
                protocolMethod = "GET";
                break;
            default:
                throw new NotImplementedException();
        }

        do
        {
            if (_cancelDownloads)
            {
                // Abort
                return 0;
            }

            try
            {
                size = await FileHelper.GetFileSizeAsync(url, protocolMethod);
                sizeRetrieved = true;
                LogAdded?.Invoke(this, new LogArgs($"Retrieved the size of the {fileTypeForLog} \"{titleForLog}\"", LogType.VerboseInfo));
            }
            catch
            {
                sizeRetrieved = false;
                if (tries + 1 < App.UserSettings.DownloadMaxTries)
                {
                    LogAdded?.Invoke(this, new LogArgs($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Try {tries + 1} of {App.UserSettings.DownloadMaxTries}", LogType.Warning));
                }
                else
                {
                    LogAdded?.Invoke(this, new LogArgs($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Hit max retries of {App.UserSettings.DownloadMaxTries}. Progress update may be wrong.", LogType.Error));
                }
            }

            tries++;
            if (!sizeRetrieved && tries < App.UserSettings.DownloadMaxTries)
            {
                await WaitForCooldownAsync(tries);
            }
        } while (!sizeRetrieved && tries < App.UserSettings.DownloadMaxTries);

        return size;
    }

    /// <summary>
    /// Returns the files to download from a list of albums.
    /// </summary>
    /// <param name="albums">The albums.</param>
    private async Task<List<TrackFile>> GetFilesToDownloadAsync(List<Album> albums)
    {
        var files = new List<TrackFile>();
        foreach (var album in albums)
        {
            LogAdded?.Invoke(this, new LogArgs($"Computing size for album \"{album.Title}\"...", LogType.Info));

            // Artwork
            if ((App.UserSettings.SaveCoverArtInTags || App.UserSettings.SaveCoverArtInFolder) && album.HasArtwork)
            {
                if (App.UserSettings.RetrieveFilesSize)
                {
                    var size = await GetFileSizeAsync(album.ArtworkUrl, album.Title, FileType.Artwork);
                    files.Add(new TrackFile(album.ArtworkUrl, 0, size));
                }
                else
                {
                    files.Add(new TrackFile(album.ArtworkUrl, 0, 0));
                }
            }

            // Tracks
            if (App.UserSettings.RetrieveFilesSize)
            {
                var tracksIndexes = Enumerable.Range(0, album.Tracks.Count).ToArray();
                await Task.WhenAll(tracksIndexes.Select(async i =>
                {
                    var size = await GetFileSizeAsync(album.Tracks[i].Mp3Url, album.Tracks[i].Title, FileType.Track);
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
    /// <returns></returns>
    private async Task WaitForCooldownAsync(int triesNumber)
    {
        if (App.UserSettings.DownloadRetryCooldown != 0)
        {
            await Task.Delay((int)(Math.Pow(App.UserSettings.DownloadRetryExponent, triesNumber) * App.UserSettings.DownloadRetryCooldown * 1000));
        }
    }

    public event LogAddedEventHandler LogAdded;
}
