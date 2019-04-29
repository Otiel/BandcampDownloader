using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using ImageResizer;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BandcampDownloader {

    public partial class WindowMain: Window {
        /// <summary>
        /// True if there are active downloads; false otherwise.
        /// </summary>
        private Boolean _activeDownloads = false;
        /// <summary>
        /// The files to download, or being downloaded, or downloaded. Used to compute the current received bytes and the total bytes to download.
        /// </summary>
        private List<TrackFile> _filesDownload;
        /// <summary>
        /// Used to compute and display the download speed.
        /// </summary>
        private DateTime _lastDownloadSpeedUpdate;
        /// <summary>
        /// Used to compute and display the download speed.
        /// </summary>
        private long _lastTotalReceivedBytes = 0;
        /// <summary>
        /// The list of WebClients currently used to download files. Used when user clicks on 'Cancel' to abort all current downloads.
        /// </summary>
        private List<WebClient> _pendingDownloads;
        /// <summary>
        /// Used when user clicks on 'Cancel' to manage the cancellation (UI...).
        /// </summary>
        private Boolean _userCancelled;

        public WindowMain() {
            // Save DataContext for bindings (must be called before initializing UI)
            DataContext = App.UserSettings;

            InitializeLogger();
            InitializeComponent();

            // Increase the maximum of concurrent connections to be able to download more than 2 (which is the default value) files at the same time
            ServicePointManager.DefaultConnectionLimit = 50;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Check for updates
            if (App.UserSettings.CheckForUpdates) {
                Task.Factory.StartNew(() => { CheckForUpdates(); });
            }
#if DEBUG
            textBoxUrls.Text = ""
                //+ "https://projectmooncircle.bandcamp.com" /* Lots of albums (124) */ + Environment.NewLine
                + "https://goataholicskjald.bandcamp.com/album/dogma" /* #65 Downloaded size ≠ predicted */ + Environment.NewLine
                //+ "https://mstrvlk.bandcamp.com/album/-" /* #64 Album with big cover */ + Environment.NewLine
                //+ "https://mstrvlk.bandcamp.com/track/-" /* #64 Track with big cover */ + Environment.NewLine
                //+ "https://weneverlearnedtolive.bandcamp.com/album/silently-i-threw-them-skyward" /* #42 Album with lyrics */ + Environment.NewLine
                //+ "https://weneverlearnedtolive.bandcamp.com/track/shadows-in-hibernation-2" /* #42 Track with lyrics */ + Environment.NewLine
                //+ "https://goataholicskjald.bandcamp.com/track/europa" + Environment.NewLine
                //+ "https://goataholicskjald.bandcamp.com/track/epilogue" + Environment.NewLine
                //+ "https://afterdarkrecordings.bandcamp.com/album/adr-unreleased-tracks" /* #69 Album without cover */ + Environment.NewLine
                //+ "https://liluglymane.bandcamp.com/album/study-of-the-hypothesized-removable-and-or-expandable-nature-of-human-capability-and-limitations-primarily-regarding-introductory-experiences-with-new-and-exciting-technologies-by-way-of-motivati-2" /* #54 Long path */ + Environment.NewLine
                ;
#endif
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e) {
            var dialog = new System.Windows.Forms.FolderBrowserDialog {
                Description = Properties.Resources.folderBrowserDialogDescription
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                textBoxDownloadsPath.Text = dialog.SelectedPath + "\\{artist}\\{album}";
                // Force update of the settings file (it's not done unless the user gives then loses focus on the textbox)
                textBoxDownloadsPath.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }

        private void ButtonOpenSettingsWindow_Click(object sender, RoutedEventArgs e) {
            var windowSettings = new WindowSettings(_activeDownloads) {
                Owner = this,
                ShowInTaskbar = false,
            };
            windowSettings.ShowDialog();
        }

        private async void ButtonStart_Click(object sender, RoutedEventArgs e) {
            if (textBoxUrls.Text == "") {
                // No URL to look
                Log("Paste some albums URLs to be downloaded", LogType.Error);
                return;
            }

            // Set controls to "downloading..." state
            _activeDownloads = true;
            UpdateControlsState(true);

            Log("Starting download...", LogType.Info);

            await StartDownloadAsync();

            if (_userCancelled) {
                // Display message if user cancelled
                Log("Downloads cancelled by user", LogType.Info);
            }

            // Set controls to "ready" state
            _activeDownloads = false;
            UpdateControlsState(false);

            if (App.UserSettings.EnableApplicationSounds) {
                // Play a sound
                try {
                    new SoundPlayer(@"C:\Windows\Media\Windows Ding.wav").Play();
                } catch {
                }
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(Properties.Resources.messageBoxCancelDownloads, "Bandcamp Downloader", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) {
                return;
            }

            _userCancelled = true;
            Cursor = Cursors.Wait;
            Log("Cancelling downloads. Please wait...", LogType.Info);

            buttonStop.IsEnabled = false;
            progressBar.Foreground = Brushes.Red;
            progressBar.IsIndeterminate = true;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            TaskbarItemInfo.ProgressValue = 0;

            lock (_pendingDownloads) {
                // Stop current downloads
                foreach (WebClient webClient in _pendingDownloads) {
                    webClient.CancelAsync();
                }
            }

            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Displays a message if a new version is available.
        /// </summary>
        private void CheckForUpdates() {
            Version latestVersion = null;
            try {
                latestVersion = UpdatesHelper.GetLatestVersion();
            } catch (CouldNotCheckForUpdatesException) {
                Dispatcher.BeginInvoke(new Action(() => {
                    labelVersion.Content += " - " + Properties.Resources.labelVersionError;
                }));
                return;
            }

            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (currentVersion.CompareTo(latestVersion) < 0) {
                // The latest version is newer than the current one
                Dispatcher.BeginInvoke(new Action(() => {
                    labelVersion.Content = String.Format(Properties.Resources.labelVersionNewUpdateAvailable, latestVersion);
                }));
            }
        }

        /// <summary>
        /// Downloads an album.
        /// </summary>
        /// <param name="album">The album to download.</param>
        private async Task DownloadAlbumAsync(Album album) {
            if (_userCancelled) {
                // Abort
                return;
            }

            // Create directory to place track files
            try {
                Directory.CreateDirectory(album.Path);
            } catch {
                Log("An error occured when creating the album folder. Make sure you have the rights to write files in the folder you chose", LogType.Error);
                return;
            }

            TagLib.Picture artwork = null;

            // Download artwork
            if ((App.UserSettings.SaveCoverArtInTags || App.UserSettings.SaveCoverArtInFolder) && album.HasArtwork) {
                artwork = await DownloadCoverArtAsync(album);
            }

            // Download & tag tracks
            Boolean[] tracksDownloaded = new Boolean[album.Tracks.Count];
            Int32[] indexes = Enumerable.Range(0, album.Tracks.Count).ToArray();
            await Task.WhenAll(indexes.Select(async i => tracksDownloaded[i] = await DownloadAndTagTrackAsync(album, album.Tracks[i], artwork)));

            // Create playlist file
            if (App.UserSettings.CreatePlaylist && !_userCancelled) {
                PlaylistHelper.SavePlaylistForAlbum(album);
                Log($"Saved playlist for album \"{album.Title}\"", LogType.IntermediateSuccess);
            }

            if (!_userCancelled) {
                // Tasks have not been aborted
                if (tracksDownloaded.All(x => x == true)) {
                    Log($"Successfully downloaded album \"{album.Title}\"", LogType.Success);
                } else {
                    Log($"Finished downloading album \"{album.Title}\". Some tracks were not downloaded", LogType.Success);
                }
            }
        }

        /// <summary>
        /// Downloads and tags a track. Returns true if the track has been correctly downloaded; false otherwise.
        /// </summary>
        /// <param name="album">The album of the track to download.</param>
        /// <param name="track">The track to download.</param>
        /// <param name="artwork">The cover art.</param>
        private async Task<Boolean> DownloadAndTagTrackAsync(Album album, Track track, TagLib.Picture artwork) {
            Log($"Downloading track \"{track.Title}\" from url: {track.Mp3Url}", LogType.VerboseInfo);

            int tries = 0;
            Boolean trackDownloaded = false;
            TrackFile currentFile = _filesDownload.Where(f => f.Url == track.Mp3Url).First();

            if (File.Exists(track.Path)) {
                long length = new FileInfo(track.Path).Length;
                if (currentFile.Size > length - (currentFile.Size * App.UserSettings.AllowedFileSizeDifference) &&
                    currentFile.Size < length + (currentFile.Size * App.UserSettings.AllowedFileSizeDifference)) {
                    Log($"Track already exists within allowed file size range: track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\" - Skipping download!", LogType.IntermediateSuccess);
                    return false;
                }
            }

            do {
                using (var webClient = new WebClient()) {
                    switch (App.UserSettings.Proxy) {
                        case ProxyType.None:
                            webClient.Proxy = null;
                            break;
                        case ProxyType.System:
                            if (webClient.Proxy != null) {
                                webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                            }
                            break;
                        case ProxyType.Manual:
                            webClient.Proxy = new WebProxy(App.UserSettings.ProxyHttpAddress, App.UserSettings.ProxyHttpPort);
                            break;
                        default:
                            throw new NotImplementedException(); // Shouldn't happen
                    }

                    // Update progress bar when downloading
                    webClient.DownloadProgressChanged += (s, e) => {
                        currentFile.BytesReceived = e.BytesReceived;
                    };

                    // Warn & tag when downloaded
                    webClient.DownloadFileCompleted += async (s, e) => {
                        if (!e.Cancelled && e.Error == null) {
                            trackDownloaded = true;

                            if (App.UserSettings.ModifyTags) {
                                // Tag (ID3) the file when downloaded
                                var tagFile = TagLib.File.Create(track.Path);
                                tagFile = TagHelper.UpdateArtist(tagFile, album.Artist, App.UserSettings.TagArtist);
                                tagFile = TagHelper.UpdateAlbumArtist(tagFile, album.Artist, App.UserSettings.TagAlbumArtist);
                                tagFile = TagHelper.UpdateAlbumTitle(tagFile, album.Title, App.UserSettings.TagAlbumTitle);
                                tagFile = TagHelper.UpdateAlbumYear(tagFile, (uint) album.ReleaseDate.Year, App.UserSettings.TagYear);
                                tagFile = TagHelper.UpdateTrackNumber(tagFile, (uint) track.Number, App.UserSettings.TagTrackNumber);
                                tagFile = TagHelper.UpdateTrackTitle(tagFile, track.Title, App.UserSettings.TagTrackTitle);
                                tagFile = TagHelper.UpdateTrackLyrics(tagFile, track.Lyrics, App.UserSettings.TagLyrics);
                                tagFile = TagHelper.UpdateComments(tagFile, App.UserSettings.TagComments);
                                tagFile.Save();
                            }

                            if (App.UserSettings.SaveCoverArtInTags && artwork != null) {
                                // Save cover in tags when downloaded
                                var tagFile = TagLib.File.Create(track.Path);
                                tagFile.Tag.Pictures = new TagLib.IPicture[1] { artwork };
                                tagFile.Save();
                            }

                            // Note the file as downloaded
                            currentFile.Downloaded = true;
                            Log($"Downloaded track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\"", LogType.IntermediateSuccess);
                        } else if (!e.Cancelled && e.Error != null) {
                            if (tries + 1 < App.UserSettings.DownloadMaxTries) {
                                Log($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Try {tries + 1} of {App.UserSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Unable to download track \"{Path.GetFileName(track.Path)}\" from album \"{album.Title}\". Hit max retries of {App.UserSettings.DownloadMaxTries}", LogType.Error);
                            }
                        } // Else the download has been cancelled (by the user)

                        tries++;
                        if (!trackDownloaded && tries < App.UserSettings.DownloadMaxTries) {
                            await WaitForCooldownAsync(tries);
                        }
                    };

                    lock (_pendingDownloads) {
                        if (_userCancelled) {
                            // Abort
                            return false;
                        }
                        // Register current download
                        _pendingDownloads.Add(webClient);
                    }

                    // Start download
                    try {
                        await webClient.DownloadFileTaskAsync(track.Mp3Url, track.Path);
                    } catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled) {
                        // Downloads cancelled by the user
                        // Do nothing
                    }

                    lock (_pendingDownloads) {
                        _pendingDownloads.Remove(webClient);
                    }
                }
            } while (!trackDownloaded && tries < App.UserSettings.DownloadMaxTries);

            return trackDownloaded;
        }

        /// <summary>
        /// Downloads and returns the cover art of the specified album.
        /// Depending on UserSettings, save the cover art in the album folder.
        /// </summary>
        /// <param name="album">The album.</param>
        private async Task<TagLib.Picture> DownloadCoverArtAsync(Album album) {
            TagLib.Picture artworkInTags = null;

            int tries = 0;
            Boolean artworkDownloaded = false;
            TrackFile currentFile = _filesDownload.Where(f => f.Url == album.ArtworkUrl).First();

            do {
                using (var webClient = new WebClient()) {
                    switch (App.UserSettings.Proxy) {
                        case ProxyType.None:
                            webClient.Proxy = null;
                            break;
                        case ProxyType.System:
                            if (webClient.Proxy != null) {
                                webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                            }
                            break;
                        case ProxyType.Manual:
                            webClient.Proxy = new WebProxy(App.UserSettings.ProxyHttpAddress, App.UserSettings.ProxyHttpPort);
                            break;
                        default:
                            throw new NotImplementedException(); // Shouldn't happen
                    }

                    // Update progress bar when downloading
                    webClient.DownloadProgressChanged += (s, e) => {
                        currentFile.BytesReceived = e.BytesReceived;
                    };

                    // Warn when downloaded
                    webClient.DownloadFileCompleted += async (s, e) => {
                        if (!e.Cancelled && e.Error == null) {
                            artworkDownloaded = true;

                            // Convert/resize artwork to be saved in album folder
                            if (App.UserSettings.SaveCoverArtInFolder && (App.UserSettings.CoverArtInFolderConvertToJpg || App.UserSettings.CoverArtInFolderResize)) {
                                var settings = new ResizeSettings();
                                if (App.UserSettings.CoverArtInFolderConvertToJpg) {
                                    settings.Format = "jpg";
                                    settings.Quality = 90;
                                }
                                if (App.UserSettings.CoverArtInFolderResize) {
                                    settings.MaxHeight = App.UserSettings.CoverArtInFolderMaxSize;
                                    settings.MaxWidth = App.UserSettings.CoverArtInFolderMaxSize;
                                }

                                await Task.Run(() => {
                                    ImageBuilder.Current.Build(album.ArtworkTempPath, album.ArtworkPath, settings); // Save it to the album folder
                                });
                            } else if (App.UserSettings.SaveCoverArtInFolder) {
                                File.Copy(album.ArtworkTempPath, album.ArtworkPath, true);
                            }

                            // Convert/resize artwork to be saved in tags
                            if (App.UserSettings.SaveCoverArtInTags && (App.UserSettings.CoverArtInTagsConvertToJpg || App.UserSettings.CoverArtInTagsResize)) {
                                var settings = new ResizeSettings();
                                if (App.UserSettings.CoverArtInTagsConvertToJpg) {
                                    settings.Format = "jpg";
                                    settings.Quality = 90;
                                }
                                if (App.UserSettings.CoverArtInTagsResize) {
                                    settings.MaxHeight = App.UserSettings.CoverArtInTagsMaxSize;
                                    settings.MaxWidth = App.UserSettings.CoverArtInTagsMaxSize;
                                }

                                await Task.Run(() => {
                                    ImageBuilder.Current.Build(album.ArtworkTempPath, album.ArtworkTempPath, settings); // Save it to %Temp%
                                });
                            }
                            artworkInTags = new TagLib.Picture(album.ArtworkTempPath) {
                                Description = "Picture"
                            };

                            try {
                                File.Delete(album.ArtworkTempPath);
                            } catch {
                                // Could not delete the file. Nevermind, it's in %Temp% folder...
                            }

                            // Note the file as downloaded
                            currentFile.Downloaded = true;
                            Log($"Downloaded artwork for album \"{album.Title}\"", LogType.IntermediateSuccess);
                        } else if (!e.Cancelled && e.Error != null) {
                            if (tries < App.UserSettings.DownloadMaxTries) {
                                Log($"Unable to download artwork for album \"{album.Title}\". Try {tries + 1} of {App.UserSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Unable to download artwork for album \"{album.Title}\". Hit max retries of {App.UserSettings.DownloadMaxTries}", LogType.Error);
                            }
                        } // Else the download has been cancelled (by the user)

                        tries++;
                        if (!artworkDownloaded && tries < App.UserSettings.DownloadMaxTries) {
                            await WaitForCooldownAsync(tries);
                        }
                    };

                    lock (_pendingDownloads) {
                        if (_userCancelled) {
                            // Abort
                            return null;
                        }
                        // Register current download
                        _pendingDownloads.Add(webClient);
                    }

                    // Start download
                    try {
                        await webClient.DownloadFileTaskAsync(album.ArtworkUrl, album.ArtworkTempPath);
                    } catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled) {
                        // Downloads cancelled by the user
                        // Do nothing
                    }

                    lock (_pendingDownloads) {
                        _pendingDownloads.Remove(webClient);
                    }
                }
            } while (!artworkDownloaded && tries < App.UserSettings.DownloadMaxTries);

            return artworkInTags;
        }

        /// <summary>
        /// Returns the albums located at the specified URLs.
        /// </summary>
        /// <param name="urls">The URLs.</param>
        private async Task<List<Album>> GetAlbumsAsync(List<String> urls) {
            var albums = new List<Album>();

            foreach (String url in urls) {
                Log($"Retrieving album data for {url}", LogType.Info);

                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient() { Encoding = Encoding.UTF8 }) {
                    switch (App.UserSettings.Proxy) {
                        case ProxyType.None:
                            webClient.Proxy = null;
                            break;
                        case ProxyType.System:
                            if (webClient.Proxy != null) {
                                webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                            }
                            break;
                        case ProxyType.Manual:
                            webClient.Proxy = new WebProxy(App.UserSettings.ProxyHttpAddress, App.UserSettings.ProxyHttpPort);
                            break;
                        default:
                            throw new NotImplementedException(); // Shouldn't happen
                    }

                    if (_userCancelled) {
                        // Abort
                        return new List<Album>();
                    }

                    try {
                        htmlCode = await webClient.DownloadStringTaskAsync(url);
                    } catch {
                        Log($"Could not retrieve data for {url}", LogType.Error);
                        continue;
                    }
                }

                // Get info on album
                try {
                    albums.Add(BandcampHelper.GetAlbum(htmlCode));
                } catch {
                    Log($"Could not retrieve album info for {url}", LogType.Error);
                    continue;
                }
            }

            return albums;
        }

        /// <summary>
        /// Returns the artists discography from any URL (artist, album, track).
        /// </summary>
        /// <param name="urls">The URLs.</param>
        private async Task<List<String>> GetArtistDiscographyAsync(List<String> urls) {
            var albumsUrls = new List<String>();

            foreach (String url in urls) {
                Log($"Retrieving artist discography from {url}", LogType.Info);

                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient() { Encoding = Encoding.UTF8 }) {
                    switch (App.UserSettings.Proxy) {
                        case ProxyType.None:
                            webClient.Proxy = null;
                            break;
                        case ProxyType.System:
                            if (webClient.Proxy != null) {
                                webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                            }
                            break;
                        case ProxyType.Manual:
                            webClient.Proxy = new WebProxy(App.UserSettings.ProxyHttpAddress, App.UserSettings.ProxyHttpPort);
                            break;
                        default:
                            throw new NotImplementedException(); // Shouldn't happen
                    }

                    if (_userCancelled) {
                        // Abort
                        return new List<String>();
                    }

                    try {
                        htmlCode = await webClient.DownloadStringTaskAsync(url);
                    } catch {
                        Log($"Could not retrieve data for {url}", LogType.Error);
                        continue;
                    }
                }

                // Get artist "music" bandcamp page (http://artist.bandcamp.com/music)
                var regex = new Regex("band_url = \"(?<url>.*)\"");
                if (!regex.IsMatch(htmlCode)) {
                    Log($"No discography could be found on {url}. Try to uncheck the \"Download artist discography\" option", LogType.Error);
                    continue;
                }
                String artistMusicPage = regex.Match(htmlCode).Groups["url"].Value + "/music";

                // Retrieve artist "music" page HTML source code
                using (var webClient = new WebClient() { Encoding = Encoding.UTF8 }) {
                    switch (App.UserSettings.Proxy) {
                        case ProxyType.None:
                            webClient.Proxy = null;
                            break;
                        case ProxyType.System:
                            if (webClient.Proxy != null) {
                                webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                            }
                            break;
                        case ProxyType.Manual:
                            webClient.Proxy = new WebProxy(App.UserSettings.ProxyHttpAddress, App.UserSettings.ProxyHttpPort);
                            break;
                        default:
                            throw new NotImplementedException(); // Shouldn't happen
                    }

                    if (_userCancelled) {
                        // Abort
                        return new List<String>();
                    }

                    try {
                        htmlCode = await webClient.DownloadStringTaskAsync(artistMusicPage);
                    } catch {
                        Log($"Could not retrieve data for {artistMusicPage}", LogType.Error);
                        continue;
                    }
                }

                // Get albums referred on the page
                regex = new Regex("TralbumData.*\n.*url:.*'/music'\n");
                if (!regex.IsMatch(htmlCode)) {
                    // This seem to be a one-album artist with no "music" page => URL redirects to the unique album URL
                    albumsUrls.Add(url);
                } else {
                    // We are on a real "music" page
                    try {
                        albumsUrls.AddRange(BandcampHelper.GetAlbumsUrl(htmlCode));
                    } catch (NoAlbumFoundException) {
                        Log($"No referred album could be found on {artistMusicPage}. Try to uncheck the \"Download artist discography\" option", LogType.Error);
                        continue;
                    }
                }
            }

            return albumsUrls;
        }

        /// <summary>
        /// Returns the size of the file located at the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="titleForLog">The title of the file to be displayed in the log.</param>
        /// <param name="fileType">The type of the file.</param>
        private async Task<long> GetFileSizeAsync(String url, String titleForLog, FileType fileType) {
            long size = 0;
            Boolean sizeRetrieved;
            int tries = 0;
            String fileTypeForLog;
            String protocolMethod;

            switch (fileType) {
                case FileType.Artwork:
                    fileTypeForLog = "cover art file for album";
                    protocolMethod = "HEAD";
                    break;
                case FileType.Track:
                    fileTypeForLog = "MP3 file for the track";
                    // Using the HEAD method on tracks urls does not work (Error 405: Method not allowed)
                    // Surprisingly, using the GET method does not seem to download the whole file, so we will use it to retrieve the mp3 sizes
                    protocolMethod = "GET";
                    break;
                default:
                    throw new NotImplementedException();
            }

            do {
                if (_userCancelled) {
                    // Abort
                    return 0;
                }

                try {
                    size = await FileHelper.GetFileSizeAsync(url, protocolMethod);
                    sizeRetrieved = true;
                    Log($"Retrieved the size of the {fileTypeForLog} \"{titleForLog}\"", LogType.VerboseInfo);
                } catch {
                    sizeRetrieved = false;
                    if (tries + 1 < App.UserSettings.DownloadMaxTries) {
                        Log($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Try {tries + 1} of {App.UserSettings.DownloadMaxTries}", LogType.Warning);
                    } else {
                        Log($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Hit max retries of {App.UserSettings.DownloadMaxTries}. Progress update may be wrong.", LogType.Error);
                    }
                }

                tries++;
                if (!sizeRetrieved && tries < App.UserSettings.DownloadMaxTries) {
                    await WaitForCooldownAsync(tries);
                }
            } while (!sizeRetrieved && tries < App.UserSettings.DownloadMaxTries);

            return size;
        }

        /// <summary>
        /// Returns the files to download from a list of albums.
        /// </summary>
        /// <param name="albums">The albums.</param>
        private async Task<List<TrackFile>> GetFilesToDownloadAsync(List<Album> albums) {
            var files = new List<TrackFile>();
            foreach (Album album in albums) {
                Log($"Computing size for album \"{album.Title}\"...", LogType.Info);

                // Artwork
                if ((App.UserSettings.SaveCoverArtInTags || App.UserSettings.SaveCoverArtInFolder) && album.HasArtwork) {
                    if (App.UserSettings.RetrieveFilesSize) {
                        long size = await GetFileSizeAsync(album.ArtworkUrl, album.Title, FileType.Artwork);
                        files.Add(new TrackFile(album.ArtworkUrl, 0, size));
                    } else {
                        files.Add(new TrackFile(album.ArtworkUrl, 0, 0));
                    }
                }

                // Tracks
                if (App.UserSettings.RetrieveFilesSize) {
                    Int32[] tracksIndexes = Enumerable.Range(0, album.Tracks.Count).ToArray();
                    await Task.WhenAll(tracksIndexes.Select(async i => {
                        long size = await GetFileSizeAsync(album.Tracks[i].Mp3Url, album.Tracks[i].Title, FileType.Track);
                        files.Add(new TrackFile(album.Tracks[i].Mp3Url, 0, size));
                    }));
                } else {
                    foreach (Track track in album.Tracks) {
                        files.Add(new TrackFile(track.Mp3Url, 0, 0));
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Initializes the logger component.
        /// </summary>
        private void InitializeLogger() {
            var fileTarget = new FileTarget() {
                FileName = Constants.LogFilePath,
                Layout = "${longdate}  ${level:uppercase=true:padding=-5:padCharacter= }  ${message}",
                ArchiveAboveSize = Constants.MaxLogSize,
                MaxArchiveFiles = 1,
            };

            var config = new LoggingConfiguration();
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        private void LabelVersion_MouseDown(object sender, MouseButtonEventArgs e) {
            Process.Start(Constants.ProjectWebsite);
        }

        /// <summary>
        /// Logs to file and displays the specified message in the log textbox.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logType">The log type.</param>
        private void Log(String message, LogType logType) {
            // Log to file
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Log(logType.ToNLogLevel(), message);

            // Log to window
            if (App.UserSettings.ShowVerboseLog || logType == LogType.Error || logType == LogType.Info || logType == LogType.IntermediateSuccess || logType == LogType.Success) {
                // Time
                var textRange = new TextRange(richTextBoxLog.Document.ContentEnd, richTextBoxLog.Document.ContentEnd) {
                    Text = DateTime.Now.ToString("HH:mm:ss") + " "
                };
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
                // Message
                textRange = new TextRange(richTextBoxLog.Document.ContentEnd, richTextBoxLog.Document.ContentEnd) {
                    Text = message
                };
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, LogHelper.GetColor(logType));
                // Line break
                richTextBoxLog.AppendText(Environment.NewLine);

                if (richTextBoxLog.IsScrolledToEnd()) {
                    richTextBoxLog.ScrollToEnd();
                }
            }
        }

        private async Task StartDownloadAsync() {
            _userCancelled = false;
            _pendingDownloads = new List<WebClient>();

            // Get user inputs
            var userUrls = textBoxUrls.Text.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            userUrls = userUrls.Distinct().ToList();

            var urls = new List<String>();
            var albums = new List<Album>();

            // Get URLs of albums to download
            if (App.UserSettings.DownloadArtistDiscography) {
                urls = await GetArtistDiscographyAsync(userUrls);
            } else {
                urls = userUrls;
            }
            urls = urls.Distinct().ToList();

            // Get info on albums
            albums = await GetAlbumsAsync(urls);

            // Save the files to download and get their size (we'll need this list to update the progressBar)
            _filesDownload = await GetFilesToDownloadAsync(albums);

            // Set progressBar max value
            long maxProgressBarValue;
            if (App.UserSettings.RetrieveFilesSize) {
                maxProgressBarValue = _filesDownload.Sum(f => f.Size); // Bytes to download
            } else {
                maxProgressBarValue = _filesDownload.Count; // Number of files to download
            }
            if (maxProgressBarValue > 0) {
                progressBar.IsIndeterminate = false;
                progressBar.Maximum = maxProgressBarValue;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            }

            // Start timer to update progress on UI
            var updateProgressTimer = new DispatcherTimer() {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            updateProgressTimer.Tick += UpdateProgressTimer_Tick;
            updateProgressTimer.Start();

            // Start timer to update download speed on UI
            var updateDownloadSpeedTimer = new DispatcherTimer() {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            updateDownloadSpeedTimer.Tick += UpdateDownloadSpeedTimer_Tick; ;
            updateDownloadSpeedTimer.Start();

            // Start downloading albums
            if (App.UserSettings.DownloadOneAlbumAtATime) {
                // Download one album at a time
                foreach (Album album in albums) {
                    await DownloadAlbumAsync(album);
                }
            } else {
                // Parallel download
                Int32[] albumsIndexes = Enumerable.Range(0, albums.Count).ToArray();
                await Task.WhenAll(albumsIndexes.Select(i => DownloadAlbumAsync(albums[i])));
            }

            // Stop timers
            updateProgressTimer.Stop();
            updateDownloadSpeedTimer.Stop();

            // Update progress one last time to make sure the downloaded bytes displayed on UI is up-to-date
            UpdateProgress();
        }

        /// <summary>
        /// Updates the state of the controls.
        /// </summary>
        /// <param name="downloadStarted">True if the download just started; false if it just stopped.</param>
        private void UpdateControlsState(Boolean downloadStarted) {
            if (downloadStarted) {
                // We just started the download
                buttonBrowse.IsEnabled = false;
                buttonStart.IsEnabled = false;
                buttonStop.IsEnabled = true;
                checkBoxDownloadDiscography.IsEnabled = false;
                labelProgress.Content = "";
                progressBar.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF01D328")); // Green
                progressBar.IsIndeterminate = true;
                progressBar.Value = progressBar.Minimum;
                richTextBoxLog.Document.Blocks.Clear();
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                TaskbarItemInfo.ProgressValue = 0;
                textBoxDownloadsPath.IsReadOnly = true;
                textBoxUrls.IsReadOnly = true;
            } else {
                // We just finished the download (or user has cancelled)
                buttonBrowse.IsEnabled = true;
                buttonStart.IsEnabled = true;
                buttonStop.IsEnabled = false;
                checkBoxDownloadDiscography.IsEnabled = true;
                labelDownloadSpeed.Content = "";
                progressBar.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF01D328")); // Green
                progressBar.IsIndeterminate = false;
                progressBar.Value = progressBar.Minimum;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                TaskbarItemInfo.ProgressValue = 0;
                textBoxDownloadsPath.IsReadOnly = false;
                textBoxUrls.IsReadOnly = false;
            }
        }

        /// <summary>
        /// Updates the download speed on UI.
        /// </summary>
        private void UpdateDownloadSpeed() {
            DateTime now = DateTime.Now;

            // Compute new progress values
            long totalReceivedBytes = _filesDownload.Sum(f => f.BytesReceived);

            Double bytesPerSecond =
                ((Double) (totalReceivedBytes - _lastTotalReceivedBytes)) /
                (now - _lastDownloadSpeedUpdate).TotalSeconds;
            _lastTotalReceivedBytes = totalReceivedBytes;
            _lastDownloadSpeedUpdate = now;

            // Update download speed on UI
            labelDownloadSpeed.Content = (bytesPerSecond / 1024).ToString("0.0") + " kB/s";
        }

        private void UpdateDownloadSpeedTimer_Tick(object sender, EventArgs e) {
            UpdateDownloadSpeed();
        }

        /// <summary>
        /// Updates the progress label on UI.
        /// </summary>
        private void UpdateProgress() {
            if (!_userCancelled) {
                // Compute new progress values
                long totalReceivedBytes = _filesDownload.Sum(f => f.BytesReceived);
                long bytesToDownload = _filesDownload.Sum(f => f.Size);

                // Update progress label
                labelProgress.Content =
                    ((Double) totalReceivedBytes / (1024 * 1024)).ToString("0.00") + " MB" +
                    (App.UserSettings.RetrieveFilesSize ? (" / " + ((Double) bytesToDownload / (1024 * 1024)).ToString("0.00") + " MB") : "");

                if (App.UserSettings.RetrieveFilesSize) {
                    // Update progress bar based on bytes received
                    progressBar.Value = totalReceivedBytes;
                    // Taskbar progress is between 0 and 1
                    TaskbarItemInfo.ProgressValue = totalReceivedBytes / progressBar.Maximum;
                } else {
                    Double downloadedFilesCount = _filesDownload.Count(f => f.Downloaded);
                    // Update progress bar based on downloaded files
                    progressBar.Value = downloadedFilesCount;
                    // Taskbar progress is between 0 and count of files to download
                    TaskbarItemInfo.ProgressValue = downloadedFilesCount / progressBar.Maximum;
                }
            }
        }

        private void UpdateProgressTimer_Tick(object sender, EventArgs e) {
            UpdateProgress();
        }

        /// <summary>
        /// Waits for a "cooldown" time, computed from the specified number of download tries.
        /// </summary>
        /// <param name="triesNumber">The times count we tried to download the same file.</param>
        /// <returns></returns>
        private async Task WaitForCooldownAsync(int triesNumber) {
            if (App.UserSettings.DownloadRetryCooldown != 0) {
                await Task.Delay((int) (Math.Pow(App.UserSettings.DownloadRetryExponent, triesNumber) * App.UserSettings.DownloadRetryCooldown * 1000));
            }
        }

        private void WindowMain_Closing(object sender, CancelEventArgs e) {
            if (_activeDownloads) {
                // There are active downloads, ask for confirmation
                if (MessageBox.Show(Properties.Resources.messageBoxCloseWindowWhenDownloading, "Bandcamp Downloader", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Cancel) {
                    // Cancel closing the window
                    e.Cancel = true;
                }
            }
        }
    }
}