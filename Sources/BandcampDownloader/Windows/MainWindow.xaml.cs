using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using ImageResizer;

namespace BandcampDownloader {

    public partial class MainWindow: Window {

        #region Fields

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
        /// <summary>
        /// Indicates if all messages should be displayed on the log.
        /// </summary>
        private Boolean verboseLog = false;

        #endregion Fields

        #region Constructor

        public MainWindow() {
            InitializeComponent();
            // Increase the maximum of concurrent connections to be able to download more than 2 (which is the default value) files at the
            // same time
            ServicePointManager.DefaultConnectionLimit = 50;
            // Load settings
            LoadSettings();
            // Hints
            textBoxUrls.Text = Constants.UrlsHint;
            textBoxUrls.Foreground = new SolidColorBrush(Colors.DarkGray);
            // Version
            labelVersion.Content = "v " + Assembly.GetEntryAssembly().GetName().Version;
        }

        #endregion Constructor

        #region Methods

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
            String directoryPath = downloadsFolder + "\\";
            try {
                Directory.CreateDirectory(directoryPath);
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
                tasks[currentIndex] = Task.Factory.StartNew(() => tracksDownloaded[currentIndex] = DownloadAndTagTrack(directoryPath, album, album.Tracks[currentIndex], tagTracks, saveCoverArtInTags, artwork));
            }

            // Wait for all tracks to be downloaded before saying the album is downloaded
            Task.WaitAll(tasks);

            if (!this.userCancelled) {
                // Tasks have not been aborted
                if (tracksDownloaded.All(x => x == true)) {
                    Log($"Successfully downloaded album \"{album.Title}\"", LogType.Success);
                } else {
                    Log($"Finished downloading album \"{album.Title}\". Some tracks could not be downloaded", LogType.Success);
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
            String trackPath = albumDirectoryPath + track.GetFileName(album.Artist);

            int tries = 0;
            Boolean trackDownloaded = false;

            do {
                var doneEvent = new AutoResetEvent(false);

                using (var webClient = new WebClient()) {
                    // Update progress bar when downloading
                    webClient.DownloadProgressChanged += (s, e) => {
                        UpdateProgress(track.Mp3Url, e.BytesReceived);
                    };

                    // Warn & tag when downloaded
                    webClient.DownloadFileCompleted += (s, e) =>
                    {
                        cooldown(tries);
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
                                tagFile.Tag.Track = (uint) track.Number;
                                tagFile.Tag.Year = (uint) album.ReleaseDate.Year;
                                tagFile.Save();
                            }

                            if (saveCoverArtInTags && artwork != null) {
                                // Save cover in tags when downloaded
                                TagLib.File tagFile = TagLib.File.Create(trackPath);
                                tagFile.Tag.Pictures = new TagLib.IPicture[1] { artwork };
                                tagFile.Save();
                            }

                            Log($"Downloaded track \"{track.GetFileName(album.Artist)}\" from album \"{album.Title}\"", LogType.IntermediateSuccess);
                        } else if (!e.Cancelled && e.Error != null) {
                            if (tries < UserSettings.DownloadMaxTries) {
                                Log($"Unable to download track \"{track.GetFileName(album.Artist)}\" from album \"{album.Title}\". Try {tries} of {UserSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Unable to download track \"{track.GetFileName(album.Artist)}\" from album \"{album.Title}\". Hit max retries of {UserSettings.DownloadMaxTries}", LogType.Error);
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
            } while (!trackDownloaded && tries < UserSettings.DownloadMaxTries);

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
            String artworkPath = ( saveCovertArtInFolder ? downloadsFolder + "\\" + album.Title.ToAllowedFileName() : Path.GetTempPath() ) + "\\" + album.Title.ToAllowedFileName() + Path.GetExtension(album.ArtworkUrl);
            TagLib.Picture artwork = null;

            int tries = 0;
            Boolean artworkDownloaded = false;

            do {
                var doneEvent = new AutoResetEvent(false);

                using (var webClient = new WebClient()) {
                    // Update progress bar when downloading
                    webClient.DownloadProgressChanged += (s, e) => {
                        UpdateProgress(album.ArtworkUrl, e.BytesReceived);
                    };

                    // Warn when downloaded
                    webClient.DownloadFileCompleted += (s, e) => {
                        if (!e.Cancelled && e.Error == null) {
                            artworkDownloaded = true;

                            // Convert/resize artwork
                            if (convertCoverArtToJpg || resizeCoverArt) {
                                var settings = new ResizeSettings();
                                if (convertCoverArtToJpg) {
                                    settings.Format = "jpg";
                                    settings.Quality = 90;
                                }
                                if (resizeCoverArt) {
                                    settings.MaxHeight = coverArtMaxSize;
                                    settings.MaxWidth = coverArtMaxSize;
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

                            Log($"Downloaded artwork for album \"{album.Title}\"", LogType.IntermediateSuccess);
                        } else if (!e.Cancelled && e.Error != null) {
                            if (tries < UserSettings.DownloadMaxTries) {
                                Log($"Unable to download artwork for album \"{album.Title}\". Try {tries} of {UserSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Unable to download artwork for album \"{album.Title}\". Hit max retries of {UserSettings.DownloadMaxTries}", LogType.Error);
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
            } while (!artworkDownloaded && tries < UserSettings.DownloadMaxTries);

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
        /// Returns the albums URLs referred in the specified URLs.
        /// </summary>
        /// <param name="urls">The URLs.</param>
        private List<String> GetAlbumsUrls(List<String> urls) {
            var albumsUrls = new List<String>();

            foreach (String url in urls) {
                Log($"Retrieving albums referred on {url}", LogType.Info);

                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient() { Encoding = Encoding.UTF8 }) {
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

                // Get albums referred on the page
                try {
                    albumsUrls.AddRange(BandcampHelper.GetAlbumsUrl(htmlCode));
                } catch (NoAlbumFoundException) {
                    Log($"No referred album could be found on {url}. Try to uncheck the \"Force download of all albums\" option", LogType.Error);
                    continue;
                }
            }

            return albumsUrls;
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

                    do {
                        if (this.userCancelled) {
                            // Abort
                            return new List<TrackFile>();
                        }
                        cooldown(tries);
                        tries++;
                        try {
                            size = FileHelper.GetFileSize(album.ArtworkUrl, "HEAD");
                            sizeRetrieved = true;
                            Log($"Retrieved the size of the cover art file for album \"{album.Title}\"", LogType.VerboseInfo);
                        } catch {
                            sizeRetrieved = false;
                            if (tries < UserSettings.DownloadMaxTries) {
                                Log($"Failed to retrieve the size of the cover art file for album \"{album.Title}\". Try {tries} of {UserSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Failed to retrieve the size of the cover art file for album \"{album.Title}\". Hit max retries of {UserSettings.DownloadMaxTries}. Progress update may be wrong.", LogType.Error);
                            }
                        }
                    } while (!sizeRetrieved && tries < UserSettings.DownloadMaxTries);

                    files.Add(new TrackFile(album.ArtworkUrl, 0, size));
                }

                // Tracks
                foreach (Track track in album.Tracks) {
                    long size = 0;
                    Boolean sizeRetrieved = false;
                    int tries = 0;

                    do {
                        if (this.userCancelled) {
                            // Abort
                            return new List<TrackFile>();
                        }
                        cooldown(tries);
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
                            if (tries < UserSettings.DownloadMaxTries) {
                                Log($"Failed to retrieve the size of the MP3 file for the track \"{track.Title}\". Try {tries} of {UserSettings.DownloadMaxTries}", LogType.Warning);
                            } else {
                                Log($"Failed to retrieve the size of the MP3 file for the track \"{track.Title}\". Hit max retries of {UserSettings.DownloadMaxTries}. Progress update may be wrong.", LogType.Error);
                            }
                        }
                    } while (!sizeRetrieved && tries < UserSettings.DownloadMaxTries);

                    files.Add(new TrackFile(track.Mp3Url, 0, size));
                }
            }
            return files;
        }

        /// <summary>
        /// If the settings file exists, reads it to sets the settings to the user interface; otherwise sets the settings to their default
        /// values.
        /// </summary>
        private void LoadSettings() {
            if (File.Exists(Constants.UserSettingsFilePath)) {
                try {
                    // Load settings from file
                    UserSettings userSettings = UserSettings.ReadFromFile(Constants.UserSettingsFilePath);
                    LoadSettingsToUserInterface(userSettings);
                } catch {
                    // Set settings to default
                    LoadSettingsToUserInterface(new UserSettings());
                    MessageBoxResult userChoice = MessageBox.Show("An error occurred while reading the settings file. Would you like to reset it to the default values?", "Bandcamp Downloader", MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (userChoice == MessageBoxResult.Yes) {
                        // Save default settings to file
                        var userSettings = new UserSettings();
                        try {
                            userSettings.SaveToFile(Constants.UserSettingsFilePath);
                        } catch {
                            MessageBox.Show($"An error occurred while trying to save the settings file. Make sure BandcampDownloader has the right to write to the file: {Constants.UserSettingsFilePath}", "Bandcamp Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            } else {
                // Set settings to default
                LoadSettingsToUserInterface(new UserSettings());
            }
        }

        /// <summary>
        /// Sets the user interface according to the specified UserSettings.
        /// </summary>
        /// <param name="userSettings">The UserSettings to use.</param>
        private void LoadSettingsToUserInterface(UserSettings userSettings) {
            checkBoxConvertToJpg.IsChecked = userSettings.ConvertCoverArtToJpg;
            checkBoxCoverArtInFolder.IsChecked = userSettings.SaveCoverArtInFolder;
            checkBoxCoverArtInTags.IsChecked = userSettings.SaveCoverArtInTags;
            checkBoxForceAlbumsDownload.IsChecked = userSettings.ForceDownloadsOfAllAlbums;
            checkBoxOneAlbumAtATime.IsChecked = userSettings.DownloadOneAlbumAtATime;
            checkBoxResizeCoverArt.IsChecked = userSettings.ResizeCoverArt;
            checkBoxTag.IsChecked = userSettings.TagTracks;
            checkBoxVerboseLog.IsChecked = userSettings.ShowVerboseLog;
            textBoxCoverArtMaxSize.Text = userSettings.CoverArtMaxSize;
            textBoxDownloadsLocation.Text = userSettings.DownloadsLocation;
        }

        /// <summary>
        /// Displays the specified message in the log with the specified color.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="color">The color.</param>
        private void Log(String message, LogType logType) {
            if (!verboseLog && ( logType == LogType.Warning || logType == LogType.VerboseInfo )) {
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
                richTextBoxLog.ScrollToEnd();
                richTextBoxLog.AppendText(Environment.NewLine);
            }));
        }

        /// <summary>
        /// Saves the current settings to the settings file.
        /// </summary>
        private void SaveCurrentSettingsToFile() {
            var userSettings = new UserSettings() {
                ConvertCoverArtToJpg = checkBoxConvertToJpg.IsChecked.Value,
                CoverArtMaxSize = textBoxCoverArtMaxSize.Text,
                DownloadOneAlbumAtATime = checkBoxOneAlbumAtATime.IsChecked.Value,
                DownloadsLocation = textBoxDownloadsLocation.Text,
                ForceDownloadsOfAllAlbums = checkBoxForceAlbumsDownload.IsChecked.Value,
                ResizeCoverArt = checkBoxResizeCoverArt.IsChecked.Value,
                SaveCoverArtInFolder = checkBoxCoverArtInFolder.IsChecked.Value,
                SaveCoverArtInTags = checkBoxCoverArtInTags.IsChecked.Value,
                ShowVerboseLog = checkBoxVerboseLog.IsChecked.Value,
                TagTracks = checkBoxTag.IsChecked.Value,
            };

            try {
                userSettings.SaveToFile(Constants.UserSettingsFilePath);
            } catch {
                Log($"An error occurred while trying to save the settings file. Make sure BandcampDownloader has the right to write to the file: {Constants.UserSettingsFilePath}", LogType.Error);
                return;
            }

            Log($"Settings have been successfully saved to file: {Constants.UserSettingsFilePath}", LogType.Success);
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
                    checkBoxForceAlbumsDownload.IsEnabled = false;
                    checkBoxConvertToJpg.IsEnabled = false;
                    checkBoxResizeCoverArt.IsEnabled = false;
                    textBoxCoverArtMaxSize.IsEnabled = false;
                    labelPixels.IsEnabled = false;
                    labelCoverArt.IsEnabled = false;
                } else {
                    // We just finished the download (or user has cancelled)
                    buttonStart.IsEnabled = true;
                    buttonStop.IsEnabled = false;
                    buttonBrowse.IsEnabled = true;
                    textBoxUrls.IsReadOnly = false;
                    progressBar.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF01D328")); // Green
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = progressBar.Minimum;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    TaskbarItemInfo.ProgressValue = 0;
                    textBoxDownloadsLocation.IsReadOnly = false;
                    checkBoxCoverArtInFolder.IsEnabled = true;
                    checkBoxCoverArtInTags.IsEnabled = true;
                    checkBoxTag.IsEnabled = true;
                    checkBoxOneAlbumAtATime.IsEnabled = true;
                    checkBoxForceAlbumsDownload.IsEnabled = true;
                    labelDownloadSpeed.Content = "";
                    checkBoxConvertToJpg.IsEnabled = true;
                    checkBoxResizeCoverArt.IsEnabled = true;
                    textBoxCoverArtMaxSize.IsEnabled = true;
                    labelPixels.IsEnabled = true;
                    labelCoverArt.IsEnabled = true;
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
                        ( (Double) ( totalReceivedBytes - this.lastTotalReceivedBytes ) ) /
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
                            ( (Double) totalReceivedBytes / ( 1024 * 1024 ) ).ToString("0.00") + " MB / " +
                            ( (Double) bytesToDownload / ( 1024 * 1024 ) ).ToString("0.00") + " MB";
                        // Update progress bar
                        progressBar.Value = totalReceivedBytes;
                        // Taskbar progress is between 0 and 1
                        TaskbarItemInfo.ProgressValue = totalReceivedBytes / progressBar.Maximum;
                    }
                }));
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

        private void buttonSaveSettingsToFile_Click(object sender, RoutedEventArgs e) {
            SaveCurrentSettingsToFile();
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

            // Get options
            Boolean tagTracks = checkBoxTag.IsChecked.Value;
            Boolean saveCoverArtInTags = checkBoxCoverArtInTags.IsChecked.Value;
            Boolean saveCoverArtInFolder = checkBoxCoverArtInFolder.IsChecked.Value;
            Boolean convertCoverArtToJpg = checkBoxConvertToJpg.IsChecked.Value;
            Boolean resizeCoverArt = checkBoxResizeCoverArt.IsChecked.Value;
            Boolean oneAlbumAtATime = checkBoxOneAlbumAtATime.IsChecked.Value;
            Boolean forceAlbumsDownload = checkBoxForceAlbumsDownload.IsChecked.Value;
            String downloadsFolder = textBoxDownloadsLocation.Text;
            this.pendingDownloads = new List<WebClient>();

            // Set controls to "downloading..." state
            UpdateControlsState(true);

            Log("Starting download...", LogType.Info);

            // Get user inputs
            List<String> userUrls = textBoxUrls.Text.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            userUrls = userUrls.Distinct().ToList();

            var urls = new List<String>();
            var albums = new List<Album>();

            Task.Factory.StartNew(() => {
                // Get URLs of albums to download
                if (forceAlbumsDownload) {
                    urls = GetAlbumsUrls(userUrls);
                } else {
                    urls = userUrls;
                }
                urls = urls.Distinct().ToList();
            }).ContinueWith(x => {
                // Get info on albums
                albums = GetAlbums(urls);
            }).ContinueWith(x => {
                // Save files to download (we'll need the list to update the progressBar)
                this.filesDownload = GetFilesToDownload(albums, saveCoverArtInTags || saveCoverArtInFolder);
            }).ContinueWith(x => {
                // Set progressBar max value
                long bytesToDownload = this.filesDownload.Sum(f => f.Size);
                if (bytesToDownload > 0) {
                    this.Dispatcher.Invoke(new Action(() => {
                        progressBar.IsIndeterminate = false;
                        progressBar.Maximum = bytesToDownload;
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    }));
                }
            }).ContinueWith(x => {
                // Start downloading albums
                if (oneAlbumAtATime) {
                    // Download one album at a time
                    foreach (Album album in albums) {
                        DownloadAlbum(album, downloadLocationParse(downloadsFolder, album), tagTracks, saveCoverArtInTags, saveCoverArtInFolder, convertCoverArtToJpg, resizeCoverArt, coverArtMaxSize);
                    }
                } else {
                    // Parallel download
                    Task[] tasks = new Task[albums.Count];
                    for (int i = 0; i < albums.Count; i++) {
                        Album album = albums[i]; // Mandatory or else => race condition
                        tasks[i] = Task.Factory.StartNew(() =>
                            DownloadAlbum(album, downloadLocationParse(downloadsFolder, album), tagTracks, saveCoverArtInTags, saveCoverArtInFolder, convertCoverArtToJpg, resizeCoverArt, coverArtMaxSize));
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
                UpdateControlsState(false);
                // Play a sound
                try {
                    ( new SoundPlayer(@"C:\Windows\Media\Windows Ding.wav") ).Play();
                } catch {
                }
            });
        }

        private string downloadLocationParse(string downloadLocation, Album album){
            downloadLocation = downloadLocation.Replace("{artist}", album.Artist.ToAllowedFileName());
            downloadLocation = downloadLocation.Replace("{album}", album.Title.ToAllowedFileName());
            return downloadLocation;
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
            if (checkBoxCoverArtInFolder == null || checkBoxCoverArtInTags == null) {
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

        private void checkBoxVerboseLog_CheckedChanged(object sender, RoutedEventArgs e) {
            this.verboseLog = checkBoxVerboseLog.IsChecked.Value;
        }

        private void checkBoxVerboseLog_MouseEnter(object sender, MouseEventArgs e) {
            checkBoxVerboseLog.Opacity = 1;
        }

        private void checkBoxVerboseLog_MouseLeave(object sender, MouseEventArgs e) {
            checkBoxVerboseLog.Opacity = 0.5;
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

        private void cooldown(int NumTries)
        {
            if (UserSettings.DownloadRetryCooldown != 0)
                Thread.Sleep((int) ((Math.Pow(UserSettings.DownloadRetryExponential, NumTries))*UserSettings.DownloadRetryCooldown*1000));
        }

        #endregion Events
    }
}