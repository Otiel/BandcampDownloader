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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Config.Net;
using ImageResizer;

namespace BandcampDownloader {

    public partial class MainWindow: Window {

        #region Fields

        public UserSettings userSettings = new ConfigurationBuilder<UserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
        /// <summary>
        /// Indicates if there are active downloads
        /// </summary>
        private Boolean activeDownloads = false;
        /// <summary>
        /// The files to download, or being downloaded, or downloaded. Used to compute the current received bytes and the total bytes to
        /// download.
        /// </summary>
        private List<TrackFile> filesDownload;
        /// <summary>
        /// Used to compute and display the download speed.
        /// </summary>
        private DateTime lastDownloadSpeedUpdate;
        /// <summary>
        /// Used to compute and display the download speed.
        /// </summary>
        private long lastTotalReceivedBytes = 0;
        /// <summary>
        /// Used when user clicks on 'Cancel' to abort all current downloads.
        /// </summary>
        private List<WebClient> pendingDownloads;
        /// <summary>
        /// Used when user clicks on 'Cancel' to manage the cancelation (UI...).
        /// </summary>
        private Boolean userCancelled;

        #endregion Fields

        #region Constructor

        public MainWindow() {
            InitializeSettings(false);
            InitializeComponent();

            // Increase the maximum of concurrent connections to be able to download more than 2 (which is the default value) files at the
            // same time
            ServicePointManager.DefaultConnectionLimit = 50;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Update controls status based on the settings values (possibly stored in the settings file)
            textBoxCoverArtMaxSize.IsEnabled = userSettings.ResizeCoverArt;
            if (!userSettings.SaveCoverArtInFolder && !userSettings.SaveCoverArtInTags) {
                checkBoxConvertToJpg.IsEnabled = false;
                checkBoxResizeCoverArt.IsEnabled = false;
                textBoxCoverArtMaxSize.IsEnabled = false;
            } else {
                checkBoxConvertToJpg.IsEnabled = true;
                checkBoxResizeCoverArt.IsEnabled = true;
                textBoxCoverArtMaxSize.IsEnabled = true;
            }
            // Hints
            textBoxUrls.Text = Constants.UrlsHint;
            textBoxUrls.Foreground = new SolidColorBrush(Colors.DarkGray);
            // Version
            labelVersion.Content = "v" + Assembly.GetEntryAssembly().GetName().Version;
            // Check for updates
            Task.Factory.StartNew(() => { CheckForUpdates(); });
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Displays a message if a new version is available.
        /// </summary>
        private void CheckForUpdates() {
            // Note: GitHub uses a HTTP redirect to redirect from the generic latest release page to the actual latest release page
            Boolean failedToRetrieveLatestVersion = false;

            // Retrieve the redirect page from the GitHub latest release page
            HttpWebRequest request = HttpWebRequest.CreateHttp(Constants.LatestReleaseWebsite);
            request.AllowAutoRedirect = false;
            String redirectPage = "";
            try {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    redirectPage = response.GetResponseHeader("Location");
                    // redirectPage should be like "https://github.com/Otiel/BandcampDownloader/releases/tag/vX.X.X.X"
                }
            } catch {
                failedToRetrieveLatestVersion = true;
            }

            // Extract the version number from the URL
            String latestVersionNumber = "";
            try {
                latestVersionNumber = redirectPage.Substring(redirectPage.LastIndexOf("/v") + 2); // X.X.X.X
            } catch {
                failedToRetrieveLatestVersion = true;
            }

            Version latestVersion;
            if (Version.TryParse(latestVersionNumber, out latestVersion)) {
                Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (currentVersion.CompareTo(latestVersion) < 0) {
                    // The latest version is newer than the current one
                    Dispatcher.BeginInvoke(new Action(() => {
                        labelVersion.Content += " - A new version is available";
                    }));
                }
            } else {
                failedToRetrieveLatestVersion = true;
            }

            if (failedToRetrieveLatestVersion) {
                Dispatcher.BeginInvoke(new Action(() => {
                    labelVersion.Content += " - Could not check for updates";
                }));
            }
        }

        /// <summary>
        /// Downloads an album.
        /// </summary>
        /// <param name="album">The album to download.</param>
        /// <param name="downloadsFolder">The downloads folder.</param>
        /// <param name="tagTracks">True to tag tracks; false otherwise.</param>
        /// <param name="saveCoverArtInTags">True to save cover art in tags; false otherwise.</param>
        /// <param name="saveCovertArtInFolder">True to save cover art in the downloads folder; false otherwise.</param>
        /// <param name="convertCoverArtToJpg">True to convert the cover art to jpg; false otherwise.</param>
        /// <param name="resizeCoverArt">True to resize the covert art; false otherwise.</param>
        /// <param name="coverArtMaxSize">The maximum width/height of the cover art when resizing.</param>
        private void DownloadAlbum(Album album, String downloadsFolder, Boolean tagTracks, Boolean saveCoverArtInTags, Boolean saveCovertArtInFolder, Boolean convertCoverArtToJpg, Boolean resizeCoverArt, int coverArtMaxSize) {
            if (this.userCancelled) {
                // Abort
                return;
            }

            // Create directory to place track files
            try {
                Directory.CreateDirectory(downloadsFolder);
            } catch {
                Log("An error occured when creating the album folder. Make sure you have the rights to write files in the folder you chose", LogType.Error);
                return;
            }

            TagLib.Picture artwork = null;

            // Download artwork
            if (saveCoverArtInTags || saveCovertArtInFolder) {
                artwork = DownloadCoverArt(album, downloadsFolder, saveCovertArtInFolder, convertCoverArtToJpg, resizeCoverArt, coverArtMaxSize);
            }

            // Download & tag tracks
            Task[] tasks = new Task[album.Tracks.Count];
            Boolean[] tracksDownloaded = new Boolean[album.Tracks.Count];
            for (int i = 0; i < album.Tracks.Count; i++) {
                // Temporarily save the index or we will have a race condition exception when i hits its maximum value
                int currentIndex = i;
                tasks[currentIndex] = Task.Factory.StartNew(() => tracksDownloaded[currentIndex] = DownloadAndTagTrack(downloadsFolder, album, album.Tracks[currentIndex], tagTracks, saveCoverArtInTags, artwork));
            }

            // Wait for all tracks to be downloaded before saying the album is downloaded
            Task.WaitAll(tasks);

            if (!this.userCancelled) {
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
        /// <param name="albumDirectoryPath">The path where to save the tracks.</param>
        /// <param name="album">The album of the track to download.</param>
        /// <param name="track">The track to download.</param>
        /// <param name="tagTrack">True to tag the track; false otherwise.</param>
        /// <param name="saveCoverArtInTags">True to save the cover art in the tag tracks; false otherwise.</param>
        /// <param name="artwork">The cover art.</param>
        private Boolean DownloadAndTagTrack(String albumDirectoryPath, Album album, Track track, Boolean tagTrack, Boolean saveCoverArtInTags, TagLib.Picture artwork) {
            Log($"Downloading track \"{track.Title}\" from url: {track.Mp3Url}", LogType.VerboseInfo);

            // Set location to save the file
            String trackPath = albumDirectoryPath + "\\" + GetFileName(album, track);
            if (trackPath.Length > 256) {
                // Shorten the path (Windows doesn't support a path > 256 characters)
                trackPath = albumDirectoryPath + "\\" + GetFileName(album, track).Substring(0, 3) + Path.GetExtension(trackPath);
            }
            int tries = 0;
            Boolean trackDownloaded = false;

            if (File.Exists(trackPath)) {
                long length = new FileInfo(trackPath).Length;
                foreach (TrackFile trackFile in filesDownload) {
                    if (track.Mp3Url == trackFile.Url &&
                        trackFile.Size > length - ( trackFile.Size * userSettings.AllowableFileSizeDifference ) &&
                        trackFile.Size < length + ( trackFile.Size * userSettings.AllowableFileSizeDifference )) {
                        Log($"Track already exists within allowed filesize range: track \"{GetFileName(album, track)}\" from album \"{album.Title}\" - Skipping download!", LogType.IntermediateSuccess);
                        return false;
                    }
                }
            }

            do {
                var doneEvent = new AutoResetEvent(false);

                using (var webClient = new WebClient()) {
                    if (webClient.Proxy != null)
                        webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                    // Update progress bar when downloading
                    webClient.DownloadProgressChanged += (s, e) => {
                        UpdateProgress(track.Mp3Url, e.BytesReceived);
                    };

                    // Warn & tag when downloaded
                    webClient.DownloadFileCompleted += (s, e) => {
                        WaitForCooldown(tries);
                        tries++;

                        if (!e.Cancelled && e.Error == null) {
                            trackDownloaded = true;

                            if (tagTrack) {
                                // Tag (ID3) the file when downloaded
                                TagLib.File tagFile = TagLib.File.Create(trackPath);
                                tagFile.Tag.Album = album.Title;
                                tagFile.Tag.AlbumArtists = new String[1] { album.Artist };
                                tagFile.Tag.Performers = new String[1] { album.Artist };
                                tagFile.Tag.Title = track.Title;
                                tagFile.Tag.Track = (uint)track.Number;
                                tagFile.Tag.Year = (uint)album.ReleaseDate.Year;
                                tagFile.Tag.Lyrics = track.Lyrics;
                                tagFile.Tag.Comment = "";
                                tagFile.Save();
                            }

                            if (saveCoverArtInTags && artwork != null) {
                                // Save cover in tags when downloaded
                                TagLib.File tagFile = TagLib.File.Create(trackPath);
                                tagFile.Tag.Pictures = new TagLib.IPicture[1] { artwork };
                                tagFile.Save();
                            }

                            // Note the file as downloaded
                            TrackFile currentFile = this.filesDownload.Where(f => f.Url == track.Mp3Url).First();
                            currentFile.Downloaded = true;
                            Log($"Downloaded track \"{GetFileName(album, track)}\" from album \"{album.Title}\"", LogType.IntermediateSuccess);
                        } else if (!e.Cancelled && e.Error != null) {
                            if (tries < userSettings.DownloadMaxTries) {
                                Log($"Unable to download track \"{GetFileName(album, track)}\" from album \"{album.Title}\". Try {tries} of {userSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Unable to download track \"{GetFileName(album, track)}\" from album \"{album.Title}\". Hit max retries of {userSettings.DownloadMaxTries}", LogType.Error);
                            }
                        } // Else the download has been cancelled (by the user)

                        doneEvent.Set();
                    };

                    lock (this.pendingDownloads) {
                        if (this.userCancelled) {
                            // Abort
                            return false;
                        }
                        // Register current download
                        this.pendingDownloads.Add(webClient);
                        // Start download
                        webClient.DownloadFileAsync(new Uri(track.Mp3Url), trackPath);
                    }
                    // Wait for download to be finished
                    doneEvent.WaitOne();
                    lock (this.pendingDownloads) {
                        this.pendingDownloads.Remove(webClient);
                    }
                }
            } while (!trackDownloaded && tries < userSettings.DownloadMaxTries);

            return trackDownloaded;
        }

        /// <summary>
        /// Downloads the cover art.
        /// </summary>
        /// <param name="album">The album to download.</param>
        /// <param name="downloadsFolder">The downloads folder.</param>
        /// <param name="saveCovertArtInFolder">True to save cover art in the downloads folder; false otherwise.</param>
        /// <param name="convertCoverArtToJpg">True to convert the cover art to jpg; false otherwise.</param>
        /// <param name="resizeCoverArt">True to resize the covert art; false otherwise.</param>
        /// <param name="coverArtMaxSize">The maximum width/height of the cover art when resizing.</param>
        /// <returns></returns>
        private TagLib.Picture DownloadCoverArt(Album album, String downloadsFolder, Boolean saveCovertArtInFolder, Boolean convertCoverArtToJpg, Boolean resizeCoverArt, int coverArtMaxSize) {
            // Compute path where to save artwork
            String artworkPath = (saveCovertArtInFolder ? downloadsFolder : Path.GetTempPath()) + "\\" + album.Title.ToAllowedFileName() + Path.GetExtension(album.ArtworkUrl);
            if (artworkPath.Length > 256) {
                // Shorten the path (Windows doesn't support a path > 256 characters)
                artworkPath = ( saveCovertArtInFolder ? downloadsFolder : Path.GetTempPath() ) + "\\" + album.Title.ToAllowedFileName().Substring(0, 3) + Path.GetExtension(album.ArtworkUrl);
            }

            TagLib.Picture artwork = null;

            int tries = 0;
            Boolean artworkDownloaded = false;

            do {
                var doneEvent = new AutoResetEvent(false);

                using (var webClient = new WebClient()) {
                    if (webClient.Proxy != null)
                        webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                    // Update progress bar when downloading
                    webClient.DownloadProgressChanged += (s, e) => {
                        UpdateProgress(album.ArtworkUrl, e.BytesReceived);
                    };

                    // Warn when downloaded
                    webClient.DownloadFileCompleted += (s, e) => {
                        if (!e.Cancelled && e.Error == null) {
                            artworkDownloaded = true;

                            // Convert/resize artwork
                            if (userSettings.ConvertCoverArtToJpg || userSettings.ResizeCoverArt) {
                                var settings = new ResizeSettings();
                                if (convertCoverArtToJpg) {
                                    settings.Format = "jpg";
                                    settings.Quality = 90;
                                }
                                if (resizeCoverArt) {
                                    settings.MaxHeight = userSettings.CoverArtMaxSize;
                                    settings.MaxWidth = userSettings.CoverArtMaxSize;
                                }
                                ImageBuilder.Current.Build(artworkPath, artworkPath, settings);
                            }

                            artwork = new TagLib.Picture(artworkPath) { Description = "Picture" };

                            // Delete the cover art file if it was saved in Temp
                            if (!saveCovertArtInFolder) {
                                try {
                                    System.IO.File.Delete(artworkPath);
                                } catch {
                                    // Could not delete the file. Nevermind, it's in Temp/ folder...
                                }
                            }

                            // Note the file as downloaded
                            TrackFile currentFile = this.filesDownload.Where(f => f.Url == album.ArtworkUrl).First();
                            currentFile.Downloaded = true;
                            Log($"Downloaded artwork for album \"{album.Title}\"", LogType.IntermediateSuccess);
                        } else if (!e.Cancelled && e.Error != null) {
                            if (tries < userSettings.DownloadMaxTries) {
                                Log($"Unable to download artwork for album \"{album.Title}\". Try {tries} of {userSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Unable to download artwork for album \"{album.Title}\". Hit max retries of {userSettings.DownloadMaxTries}", LogType.Error);
                            }
                        } // Else the download has been cancelled (by the user)

                        doneEvent.Set();
                    };

                    lock (this.pendingDownloads) {
                        if (this.userCancelled) {
                            // Abort
                            return null;
                        }
                        // Register current download
                        this.pendingDownloads.Add(webClient);
                        // Start download
                        webClient.DownloadFileAsync(new Uri(album.ArtworkUrl), artworkPath);
                    }

                    // Wait for download to be finished
                    doneEvent.WaitOne();
                    lock (this.pendingDownloads) {
                        this.pendingDownloads.Remove(webClient);
                    }
                }
            } while (!artworkDownloaded && tries < userSettings.DownloadMaxTries);

            return artwork;
        }

        /// <summary>
        /// Returns the albums located at the specified URLs.
        /// </summary>
        /// <param name="urls">The URLs.</param>
        private List<Album> GetAlbums(List<String> urls) {
            var albums = new List<Album>();

            foreach (String url in urls) {
                Log($"Retrieving album data for {url}", LogType.Info);

                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient() { Encoding = Encoding.UTF8 }) {
                    if (webClient.Proxy != null)
                        webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                    if (this.userCancelled) {
                        // Abort
                        return new List<Album>();
                    }

                    try {
                        htmlCode = webClient.DownloadString(url);
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
        private List<String> GetArtistDiscography(List<String> urls) {
            var albumsUrls = new List<String>();

            foreach (String url in urls) {
                Log($"Retrieving artist discography from {url}", LogType.Info);

                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient() { Encoding = Encoding.UTF8 }) {
                    if (webClient.Proxy != null)
                        webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                    if (this.userCancelled) {
                        // Abort
                        return new List<String>();
                    }

                    try {
                        htmlCode = webClient.DownloadString(url);
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
                    if (webClient.Proxy != null)
                        webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                    if (this.userCancelled) {
                        // Abort
                        return new List<String>();
                    }

                    try {
                        htmlCode = webClient.DownloadString(artistMusicPage);
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
        /// Replaces placeholders strings by the corresponding values in the specified filenameFormat location.
        /// </summary>
        /// <param name="downloadLocation">The download location to parse.</param>
        /// <param name="album">The album currently downloaded.</param>
        private String GetFileName(Album album, Track track) {
            String fileName =
                userSettings.FilenameFormat.Replace("{artist}", album.Artist)
                    .Replace("{title}", track.Title)
                    .Replace("{tracknum}", track.Number.ToString("00"));
            return fileName.ToAllowedFileName();
        }

        /// <summary>
        /// Returns the files to download from a list of albums.
        /// </summary>
        /// <param name="albums">The albums.</param>
        /// <param name="downloadCoverArt">True if the cover arts must be downloaded, false otherwise.</param>
        private List<TrackFile> GetFilesToDownload(List<Album> albums, Boolean downloadCoverArt) {
            var files = new List<TrackFile>();
            foreach (Album album in albums) {
                Log($"Computing size for album \"{album.Title}\"...", LogType.Info);

                // Artwork
                if (downloadCoverArt) {
                    long size = 0;
                    Boolean sizeRetrieved = false;
                    int tries = 0;
                    if (userSettings.RetrieveFilesizes) {
                        do {
                            if (this.userCancelled) {
                                // Abort
                                return new List<TrackFile>();
                            }
                            WaitForCooldown(tries);
                            tries++;
                            try {
                                size = FileHelper.GetFileSize(album.ArtworkUrl, "HEAD");
                                sizeRetrieved = true;
                                Log($"Retrieved the size of the cover art file for album \"{album.Title}\"", LogType.VerboseInfo);
                            } catch {
                                sizeRetrieved = false;
                                if (tries < userSettings.DownloadMaxTries) {
                                    Log($"Failed to retrieve the size of the cover art file for album \"{album.Title}\". Try {tries} of {userSettings.DownloadMaxTries}", LogType.Warning);
                                } else {
                                    Log($"Failed to retrieve the size of the cover art file for album \"{album.Title}\". Hit max retries of {userSettings.DownloadMaxTries}. Progress update may be wrong.", LogType.Error);
                                }
                            }
                        } while (!sizeRetrieved && tries < userSettings.DownloadMaxTries);
                    }
                    files.Add(new TrackFile(album.ArtworkUrl, 0, size));
                }

                // Tracks
                foreach (Track track in album.Tracks) {
                    long size = 0;
                    Boolean sizeRetrieved = false;
                    int tries = 0;
                    if (userSettings.RetrieveFilesizes)
                        do {
                            if (this.userCancelled) {
                                // Abort
                                return new List<TrackFile>();
                            }
                            WaitForCooldown(tries);
                            tries++;
                            try {
                                // Using the HEAD method on tracks urls does not work (Error 405: Method not allowed)
                                // Surprisingly, using the GET method does not seem to download the whole file, so we will use it to retrieve
                                // the mp3 sizes
                                size = FileHelper.GetFileSize(track.Mp3Url, "GET");
                                sizeRetrieved = true;
                                Log($"Retrieved the size of the MP3 file for the track \"{track.Title}\"", LogType.VerboseInfo);
                            } catch {
                                sizeRetrieved = false;
                                if (tries < userSettings.DownloadMaxTries) {
                                    Log($"Failed to retrieve the size of the MP3 file for the track \"{track.Title}\". Try {tries} of {userSettings.DownloadMaxTries}", LogType.Warning);
                                } else {
                                    Log($"Failed to retrieve the size of the MP3 file for the track \"{track.Title}\". Hit max retries of {userSettings.DownloadMaxTries}. Progress update may be wrong.", LogType.Error);
                                }
                            }
                        } while (!sizeRetrieved && tries < userSettings.DownloadMaxTries);
                    files.Add(new TrackFile(track.Mp3Url, 0, size));
                }
            }
            return files;
        }

        private void InitializeSettings(Boolean resetToDefaults) {
            if (resetToDefaults) {
                File.Delete(Constants.UserSettingsFilePath);
            }
            // Must set this before UI forms
            // Its default value cannot be set in settings as it isn't determined by a constant function
            if (String.IsNullOrEmpty(userSettings.DownloadsLocation)) {
                userSettings.DownloadsLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
            }
            userSettings = new ConfigurationBuilder<UserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
            // Save DataContext for bindings
            DataContext = userSettings;
        }

        /// <summary>
        /// Displays the specified message in the log with the specified color.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="color">The color.</param>
        private void Log(String message, LogType logType) {
            if (!userSettings.ShowVerboseLog && ( logType == LogType.Warning || logType == LogType.VerboseInfo )) {
                return;
            }

            this.Dispatcher.Invoke(new Action(() => {
                // Time
                var textRange = new TextRange(richTextBoxLog.Document.ContentEnd, richTextBoxLog.Document.ContentEnd);
                textRange.Text = DateTime.Now.ToString("HH:mm:ss") + " ";
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
                // Message
                textRange = new TextRange(richTextBoxLog.Document.ContentEnd, richTextBoxLog.Document.ContentEnd);
                textRange.Text = message;
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, LogHelper.GetColor(logType));
                // Line break

                richTextBoxLog.AppendText(Environment.NewLine);
                if (userSettings.AutoScrollLog) {
                    richTextBoxLog.ScrollToEnd();
                }
            }));
        }

        /// <summary>
        /// Replaces placeholders strings by the corresponding values in the specified download location.
        /// </summary>
        /// <param name="downloadLocation">The download location to parse.</param>
        /// <param name="album">The album currently downloaded.</param>
        private String ParseDownloadLocation(String downloadLocation, Album album) {
            downloadLocation = downloadLocation.Replace("{year}", album.ReleaseDate.Year.ToString().ToAllowedFileName());
            downloadLocation = downloadLocation.Replace("{month}", album.ReleaseDate.Month.ToString().ToAllowedFileName());
            downloadLocation = downloadLocation.Replace("{day}", album.ReleaseDate.Day.ToString().ToAllowedFileName());
            downloadLocation = downloadLocation.Replace("{artist}", album.Artist.ToAllowedFileName());
            downloadLocation = downloadLocation.Replace("{album}", album.Title.ToAllowedFileName());
            return downloadLocation;
        }

        /// <summary>
        /// Updates the state of the controls.
        /// </summary>
        /// <param name="downloadStarted">True if the download just started, false if it just stopped.</param>
        private void UpdateControlsState(Boolean downloadStarted) {
            this.Dispatcher.Invoke(new Action(() => {
                if (downloadStarted) {
                    // We just started the download
                    richTextBoxLog.Document.Blocks.Clear();
                    labelProgress.Content = "";
                    progressBar.IsIndeterminate = true;
                    progressBar.Value = progressBar.Minimum;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                    TaskbarItemInfo.ProgressValue = 0;
                    buttonStart.IsEnabled = false;
                    buttonStop.IsEnabled = true;
                    buttonBrowse.IsEnabled = false;
                    textBoxUrls.IsReadOnly = true;
                    textBoxDownloadsLocation.IsReadOnly = true;
                    checkBoxCoverArtInFolder.IsEnabled = false;
                    checkBoxCoverArtInTags.IsEnabled = false;
                    checkBoxTag.IsEnabled = false;
                    checkBoxOneAlbumAtATime.IsEnabled = false;
                    checkBoxDownloadDiscography.IsEnabled = false;
                    checkBoxConvertToJpg.IsEnabled = false;
                    checkBoxResizeCoverArt.IsEnabled = false;
                    textBoxCoverArtMaxSize.IsEnabled = false;
                    checkBoxRetrieveFilesizes.IsEnabled = false;
                } else {
                    // We just finished the download (or user has cancelled)
                    buttonStart.IsEnabled = true;
                    buttonStop.IsEnabled = false;
                    buttonBrowse.IsEnabled = true;
                    textBoxUrls.IsReadOnly = false;
                    progressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF01D328")); // Green
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = progressBar.Minimum;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    TaskbarItemInfo.ProgressValue = 0;
                    textBoxDownloadsLocation.IsReadOnly = false;
                    checkBoxCoverArtInFolder.IsEnabled = true;
                    checkBoxCoverArtInTags.IsEnabled = true;
                    checkBoxTag.IsEnabled = true;
                    checkBoxOneAlbumAtATime.IsEnabled = true;
                    checkBoxDownloadDiscography.IsEnabled = true;
                    labelDownloadSpeed.Content = "";
                    checkBoxConvertToJpg.IsEnabled = true;
                    checkBoxResizeCoverArt.IsEnabled = true;
                    textBoxCoverArtMaxSize.IsEnabled = true;
                    checkBoxRetrieveFilesizes.IsEnabled = true;
                }
            }));
        }

        /// <summary>
        /// Updates the progress messages and the progressbar.
        /// </summary>
        /// <param name="fileUrl">The URL of the file that just progressed.</param>
        /// <param name="bytesReceived">The received bytes for the specified file.</param>
        private void UpdateProgress(String fileUrl, long bytesReceived) {
            DateTime now = DateTime.Now;

            lock (this.filesDownload) {
                // Compute new progress values
                TrackFile currentFile = this.filesDownload.Where(f => f.Url == fileUrl).First();
                currentFile.BytesReceived = bytesReceived;
                long totalReceivedBytes = this.filesDownload.Sum(f => f.BytesReceived);
                long bytesToDownload = this.filesDownload.Sum(f => f.Size);
                Double downloadedFilesCount = this.filesDownload.Count(f => f.Downloaded);

                Double bytesPerSecond;
                if (this.lastTotalReceivedBytes == 0) {
                    // First time we update the progress
                    bytesPerSecond = 0;
                    this.lastTotalReceivedBytes = totalReceivedBytes;
                    this.lastDownloadSpeedUpdate = now;
                } else if (( now - this.lastDownloadSpeedUpdate ).TotalMilliseconds > 500) {
                    // Last update of progress happened more than 500 milliseconds ago
                    // We only update the download speed every 500+ milliseconds
                    bytesPerSecond =
                        ( (Double)( totalReceivedBytes - this.lastTotalReceivedBytes ) ) /
                        ( now - this.lastDownloadSpeedUpdate ).TotalSeconds;
                    this.lastTotalReceivedBytes = totalReceivedBytes;
                    this.lastDownloadSpeedUpdate = now;

                    // Update UI
                    this.Dispatcher.Invoke(new Action(() => {
                        // Update download speed
                        labelDownloadSpeed.Content = ( bytesPerSecond / 1024 ).ToString("0.0") + " kB/s";
                    }));
                }

                // Update UI
                this.Dispatcher.Invoke(new Action(() => {
                    if (!this.userCancelled) {
                        // Update progress label
                        labelProgress.Content =
                            ( (Double)totalReceivedBytes / ( 1024 * 1024 ) ).ToString("0.00") + " MB" +
                            ( userSettings.RetrieveFilesizes ? ( " / " + ( (Double)bytesToDownload / ( 1024 * 1024 ) ).ToString("0.00") + " MB" ) : "" );
                        if (userSettings.RetrieveFilesizes) {
                            // Update progress bar based on bytes received
                            progressBar.Value = totalReceivedBytes;
                            // Taskbar progress is between 0 and 1
                            TaskbarItemInfo.ProgressValue = totalReceivedBytes / progressBar.Maximum;
                        } else {
                            // Update progress bar based on downloaded files
                            progressBar.Value = downloadedFilesCount;
                            // Taskbar progress is between 0 and count of files to download
                            TaskbarItemInfo.ProgressValue = downloadedFilesCount / progressBar.Maximum;
                        }
                    }
                }));
            }
        }

        private void WaitForCooldown(int NumTries) {
            if (userSettings.DownloadRetryCooldown != 0) {
                Thread.Sleep((int)( ( Math.Pow(userSettings.DownloadRetryExponential, NumTries) ) * userSettings.DownloadRetryCooldown * 1000 ));
            }
        }

        #endregion Methods

        #region Events

        private void buttonBrowse_Click(object sender, RoutedEventArgs e) {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select the folder to save albums";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                textBoxDownloadsLocation.Text = dialog.SelectedPath;
            }
        }

        private void buttonDefaultSettings_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Reset settings to their default values?", "Bandcamp Downloader", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                InitializeSettings(true);
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e) {
            if (textBoxUrls.Text == Constants.UrlsHint) {
                // No URL to look
                Log("Paste some albums URLs to be downloaded", LogType.Error);
                return;
            }
            int coverArtMaxSize = 0;
            if (checkBoxResizeCoverArt.IsChecked.Value && !Int32.TryParse(textBoxCoverArtMaxSize.Text, out coverArtMaxSize)) {
                Log("Cover art max width/height must be an integer", LogType.Error);
                return;
            }

            this.userCancelled = false;

            this.pendingDownloads = new List<WebClient>();

            // Set controls to "downloading..." state
            this.activeDownloads = true;
            UpdateControlsState(true);

            Log("Starting download...", LogType.Info);

            // Get user inputs
            List<String> userUrls = textBoxUrls.Text.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            userUrls = userUrls.Distinct().ToList();

            var urls = new List<String>();
            var albums = new List<Album>();

            Task.Factory.StartNew(() => {
                // Get URLs of albums to download
                if (userSettings.DownloadArtistDiscography) {
                    urls = GetArtistDiscography(userUrls);
                } else {
                    urls = userUrls;
                }
                urls = urls.Distinct().ToList();
            }).ContinueWith(x => {
                // Get info on albums
                albums = GetAlbums(urls);
            }).ContinueWith(x => {
                // Save files to download (we'll need the list to update the progressBar)
                this.filesDownload = GetFilesToDownload(albums, userSettings.SaveCoverArtInTags || userSettings.SaveCoverArtInFolder);
            }).ContinueWith(x => {
                // Set progressBar max value
                long maxProgressBarValue;
                if (userSettings.RetrieveFilesizes) {
                    maxProgressBarValue = this.filesDownload.Sum(f => f.Size); // Bytes to download
                } else {
                    maxProgressBarValue = this.filesDownload.Count; // Number of files to download
                }
                if (maxProgressBarValue > 0) {
                    this.Dispatcher.Invoke(new Action(() => {
                        progressBar.IsIndeterminate = false;
                        progressBar.Maximum = maxProgressBarValue;
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    }));
                }
            }).ContinueWith(x => {
                // Start downloading albums
                if (userSettings.DownloadOneAlbumAtATime) {
                    // Download one album at a time
                    foreach (Album album in albums) {
                        DownloadAlbum(album, ParseDownloadLocation(userSettings.DownloadsLocation, album), userSettings.TagTracks, userSettings.SaveCoverArtInTags, userSettings.SaveCoverArtInFolder, userSettings.ConvertCoverArtToJpg, userSettings.ResizeCoverArt, userSettings.CoverArtMaxSize);
                    }
                } else {
                    // Parallel download
                    Task[] tasks = new Task[albums.Count];
                    for (int i = 0; i < albums.Count; i++) {
                        Album album = albums[i]; // Mandatory or else => race condition
                        tasks[i] = Task.Factory.StartNew(() =>
                            DownloadAlbum(album, ParseDownloadLocation(userSettings.DownloadsLocation, album), userSettings.TagTracks, userSettings.SaveCoverArtInTags, userSettings.SaveCoverArtInFolder, userSettings.ConvertCoverArtToJpg, userSettings.ResizeCoverArt, userSettings.CoverArtMaxSize));
                    }
                    // Wait for all albums to be downloaded
                    Task.WaitAll(tasks);
                }
            }).ContinueWith(x => {
                if (this.userCancelled) {
                    // Display message if user cancelled
                    Log("Downloads cancelled by user", LogType.Info);
                }
                // Set controls to "ready" state
                this.activeDownloads = false;
                UpdateControlsState(false);
                // Play a sound
                try {
                    ( new SoundPlayer(@"C:\Windows\Media\Windows Ding.wav") ).Play();
                } catch {
                }
            });
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Would you like to cancel all downloads?", "Bandcamp Downloader", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) {
                return;
            }

            this.userCancelled = true;
            Cursor = Cursors.Wait;
            Log("Cancelling downloads. Please wait...", LogType.Info);

            lock (this.pendingDownloads) {
                if (this.pendingDownloads.Count == 0) {
                    // Nothing to cancel
                    Cursor = Cursors.Arrow;
                    return;
                }
            }

            buttonStop.IsEnabled = false;
            progressBar.Foreground = System.Windows.Media.Brushes.Red;
            progressBar.IsIndeterminate = true;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            TaskbarItemInfo.ProgressValue = 0;

            lock (this.pendingDownloads) {
                // Stop current downloads
                foreach (WebClient webClient in this.pendingDownloads) {
                    webClient.CancelAsync();
                }
            }

            Cursor = Cursors.Arrow;
        }

        private void checkBoxResizeCoverArt_CheckedChanged(object sender, RoutedEventArgs e) {
            if (checkBoxResizeCoverArt == null || textBoxCoverArtMaxSize == null) {
                return;
            }

            textBoxCoverArtMaxSize.IsEnabled = checkBoxResizeCoverArt.IsChecked.Value;
        }

        private void checkBoxSaveCoverArt_CheckedChanged(object sender, RoutedEventArgs e) {
            if (checkBoxCoverArtInFolder == null || checkBoxCoverArtInTags == null || checkBoxConvertToJpg == null) {
                return;
            }

            if (!checkBoxCoverArtInFolder.IsChecked.Value && !checkBoxCoverArtInTags.IsChecked.Value) {
                checkBoxConvertToJpg.IsEnabled = false;
                checkBoxResizeCoverArt.IsEnabled = false;
                textBoxCoverArtMaxSize.IsEnabled = false;
            } else {
                checkBoxConvertToJpg.IsEnabled = true;
                checkBoxResizeCoverArt.IsEnabled = true;
                textBoxCoverArtMaxSize.IsEnabled = true;
            }
        }

        private void labelVersion_MouseDown(object sender, MouseButtonEventArgs e) {
            Process.Start(Constants.ProjectWebsite);
        }

        private void textBoxUrls_GotFocus(object sender, RoutedEventArgs e) {
            if (textBoxUrls.Text == Constants.UrlsHint) {
                // Erase the hint message
                textBoxUrls.Text = "";
                textBoxUrls.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void textBoxUrls_LostFocus(object sender, RoutedEventArgs e) {
            if (textBoxUrls.Text == "") {
                // Show the hint message
                textBoxUrls.Text = Constants.UrlsHint;
                textBoxUrls.Foreground = new SolidColorBrush(Colors.DarkGray);
            }
        }

        private void WindowMain_Closing(object sender, CancelEventArgs e) {
            if (this.activeDownloads) {
                // There are active downloads, ask for confirmation
                if (MessageBox.Show("There are currently active downloads. Are you sure you want to close the application and stop all downloads?", "Bandcamp Downloader", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Cancel) {
                    // Cancel closing the window
                    e.Cancel = true;
                }
            }
        }

        #endregion Events
    }
}