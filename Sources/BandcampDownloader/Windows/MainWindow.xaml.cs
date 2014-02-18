using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace BandcampDownloader {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window {

        #region Fields

        /// <summary>
        /// The files to download, or being downloaded, or downloaded
        /// Used to compute the current received bytes and the total bytes to download
        /// </summary>
        private List<File> filesDownload;

        /// <summary>
        /// Used to compute and display the download speed
        /// </summary>
        private DateTime lastDownloadSpeedUpdate;

        /// <summary>
        /// Used to compute and display the download speed
        /// </summary>
        private long lastTotalReceivedBytes = 0;

        /// <summary>
        /// Used when user clicks on 'Stop' to abort all current downloads
        /// </summary>
        private List<WebClient> pendingDownloads;

        /// <summary>
        /// Used when user clicks on 'Stop' to manage the cancelation (UI...)
        /// </summary>
        private Boolean userCancelled;
        #endregion Fields

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            // Increase the maximum of concurrent connections to be able to download more than 2
            // (which is the default value) files at the same time
            ServicePointManager.DefaultConnectionLimit = 50;
            // Default options
            textBoxDownloadsLocation.Text =
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // Hints
            textBoxUrls.Text = Constants.UrlsHint;
            textBoxUrls.Foreground = new SolidColorBrush(Colors.DarkGray);
        }

        #endregion Constructor

        #region Methods
        /// <summary>
        /// Downloads an album.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <param name="downloadsFolder">The downloads folder.</param>
        /// <param name="tagTracks">if set to <c>true</c>, tracks will be tagged.</param>
        /// <param name="saveCoverArtInTags">if set to <c>true</c>, cover art will be saved in
        /// tags.</param>
        /// <param name="saveCovertArtInFolder">if set to <c>true</c>, cover art will be saved in
        /// the downloads folder.</param>
        private void DownloadAlbum(Album album, String downloadsFolder, Boolean tagTracks,
            Boolean saveCoverArtInTags, Boolean saveCovertArtInFolder) {
            if (this.userCancelled) {
                // Abort
                return;
            }

            // Download artwork
            String artworkPath = ( saveCovertArtInFolder ?
                downloadsFolder + "\\" + album.Title.ToAllowedFileName() :
                Path.GetTempPath() ) + "\\" +
                album.Title.ToAllowedFileName() + Path.GetExtension(album.ArtworkUrl);
            var doneEvent = new AutoResetEvent(false);
            using (var webClient = new WebClient()) {
                // Update progress bar when downloading
                webClient.DownloadProgressChanged += (s, e) => {
                    UpdateProgress(album.ArtworkUrl, e.BytesReceived);
                };

                // Warn when downloaded
                webClient.DownloadFileCompleted += (s, e) => {
                    if (!e.Cancelled) {
                        Log("Downloaded artwork for album \"" + album.Title + "\"",
                            Brushes.MediumBlue);
                    }
                    doneEvent.Set();
                };

                lock (this.pendingDownloads) {
                    if (this.userCancelled) {
                        // Abort
                        return;
                    }
                    // Register current download
                    this.pendingDownloads.Add(webClient);
                    // Start download
                    webClient.DownloadFileAsync(new Uri(album.ArtworkUrl), artworkPath);
                }
            }
            // Wait for download to be finished
            doneEvent.WaitOne();

            // Create directory to place track files
            String directoryPath = downloadsFolder + "\\" + album.Title.ToAllowedFileName() + "\\";
            try {
                Directory.CreateDirectory(directoryPath);
            } catch {
                Log("An error occured when creating the album folder. Make sure you have " +
                    "the rights to write files in the folder you chose", Brushes.Red);
                return;
            }
            var artwork = new TagLib.Picture(artworkPath);

            // Download & tag tracks
            Task[] tasks = new Task[album.Tracks.Count];
            for (int i = 0; i < album.Tracks.Count; i++) {
                Track track = album.Tracks[i]; // Mandatory or else => race condition
                tasks[i] = Task.Factory.StartNew(() =>
                    DownloadAndTagTrack(album, artwork, directoryPath, track, tagTracks,
                    saveCoverArtInTags));
            }

            // Wait for all tracks to be downloaded before saying the album is downloaded
            Task.WaitAll(tasks);

            if (!this.userCancelled) {
                Log("Finished downloading album \"" + album.Title + "\"", Brushes.Green);
            }
        }

        /// <summary>
        /// Downloads and tags a track.
        /// </summary>
        /// <param name="album">The album of the track to download.</param>
        /// <param name="artwork">The cover art.</param>
        /// <param name="albumDirectoryPath">The path where to save the tracks.</param>
        /// <param name="track">The track to download.</param>
        /// <param name="tagTrack">if set to <c>true</c>, the track will be tagged..</param>
        /// <param name="saveCoverArtInTags">if set to <c>true</c>, the cover art will be saved in
        /// the track tags.</param>
        private void DownloadAndTagTrack(Album album, TagLib.Picture artwork,
            String albumDirectoryPath, Track track, Boolean tagTrack, Boolean saveCoverArtInTags) {
            // Set location to save the file
            String trackPath = albumDirectoryPath + track.GetFileName(album.Artist);

            var doneEvent = new AutoResetEvent(false);

            using (var webClient = new WebClient()) {
                // Update progress bar when downloading
                webClient.DownloadProgressChanged += (s, e) => {
                    UpdateProgress(track.Mp3Url, e.BytesReceived);
                };

                webClient.DownloadFileCompleted += (s, e) => {
                    if (!e.Cancelled && e.Error == null) {
                        if (tagTrack) {
                            // Tag (ID3) the file when downloaded
                            TagLib.File tagFile = TagLib.File.Create(trackPath);
                            tagFile.Tag.Album = album.Title;
                            tagFile.Tag.AlbumArtists = new String[1] { album.Artist };
                            tagFile.Tag.Performers = new String[1] { album.Artist };
                            tagFile.Tag.Pictures = new TagLib.IPicture[1] { artwork };
                            tagFile.Tag.Title = track.Title;
                            tagFile.Tag.Track = (uint) track.Number;
                            tagFile.Tag.Year = (uint) album.ReleaseDate.Year;
                            tagFile.Save();
                        }

                        if (saveCoverArtInTags) {
                            // Save cover in tags when downloaded
                            TagLib.File tagFile = TagLib.File.Create(trackPath);
                            tagFile.Tag.Pictures = new TagLib.IPicture[1] { artwork };
                            tagFile.Save();
                        }

                        Log("Downloaded track \"" + track.GetFileName(album.Artist) +
                            "\" from album \"" + album.Title + "\"", Brushes.MediumBlue);
                    } else if (!e.Cancelled && e.Error != null) {
                        Log("Unable to download the track \"" + track.GetFileName(album.Artist) +
                            "\" from album \"" + album.Title + "\"", Brushes.Red);
                    } // Else the download has been cancelled (by the user)

                    doneEvent.Set();
                };

                lock (this.pendingDownloads) {
                    if (this.userCancelled) {
                        // Abort
                        return;
                    }
                    // Register current download
                    this.pendingDownloads.Add(webClient);
                    // Start download
                    webClient.DownloadFileAsync(new Uri(track.Mp3Url), trackPath);
                }
            }

            // Wait for download to be finished
            doneEvent.WaitOne();
        }

        /// <summary>
        /// Returns the albums URLs referred in the specified URLs.
        /// </summary>
        /// <param name="urls">The URLs.</param>
        /// <returns>The albums URLs referred in the specified URLs.</returns>
        private List<String> GetAlbumsUrls(List<String> urls) {
            var albumsUrls = new List<String>();

            foreach (String url in urls) {
                if (this.userCancelled) {
                    // Abort
                    return new List<String>();
                }

                Log("Retrieving albums referred on " + url, Brushes.Black);

                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient()) {
                    try {
                        htmlCode = webClient.DownloadString(url);
                    } catch {
                        Log("Could not retrieve data for " + url, Brushes.Red);
                        continue;
                    }
                }

                // Get albums referred on the page
                try {
                    albumsUrls.AddRange(BandcampHelper.GetAlbumsUrl(htmlCode));
                } catch (NoAlbumFoundException) {
                    Log("No referred album could be found on " + url + ". Try " +
                        "to uncheck the \"Force download of all albums\" option", Brushes.Red);
                    continue;
                }
            }

            return albumsUrls;
        }

        /// <summary>
        /// Returns the albums located at the specified URLs.
        /// </summary>
        /// <param name="urls">The URLs.</param>
        /// <returns>The albums located at the specified URLs.</returns>
        private List<Album> GetAlbums(List<String> urls) {
            var albums = new List<Album>();

            foreach (String url in urls) {
                if (this.userCancelled) {
                    // Abort
                    return new List<Album>();
                }

                Log("Retrieving album data for " + url, Brushes.Black);

                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient()) {
                    try {
                        htmlCode = webClient.DownloadString(url);
                    } catch {
                        Log("Could not retrieve data for " + url, Brushes.Red);
                        continue;
                    }
                }

                // Get info on album
                try {
                    albums.Add(BandcampHelper.GetAlbum(htmlCode));
                } catch {
                    Log("Could not retrieve album info for " + url, Brushes.Red);
                    continue;
                }
            }

            return albums;
        }

        /// <summary>
        /// Returns the files to download from a list of albums.
        /// </summary>
        /// <param name="albums">The albums.</param>
        /// <returns>The files to download.</returns>
        private List<File> GetFilesToDownload(List<Album> albums) {
            var files = new List<File>();
            foreach (Album album in albums) {
                if (this.userCancelled) {
                    // Abort
                    return new List<File>();
                }

                Log("Computing size for album \"" + album.Title + "\"", Brushes.Black);

                // Artwork
                long size = 0;
                try {
                    size = FileHelper.GetFileSize(album.ArtworkUrl, "HEAD");
                } catch {
                    Log("Failed to retrieve the size of the cover art file for album \"" +
                        album.Title + "\". Progress update may be wrong.", Brushes.OrangeRed);
                }
                files.Add(new File(album.ArtworkUrl, 0, size));

                // Tracks
                foreach (Track track in album.Tracks) {
                    size = 0;
                    try {
                        // Using the HEAD method on tracks urls does not work
                        // (Error 405: Method not allowed)
                        // Surprisingly, using the GET method does not seem to download the whole
                        // file, so we will use it to retrieve the mp3 sizes
                        size = FileHelper.GetFileSize(track.Mp3Url, "GET");
                    } catch {
                        Log("Failed to retrieve the size of the MP3 file for the track \"" +
                            track.Title + "\". Progress update may be wrong.", Brushes.OrangeRed);
                    }

                    files.Add(new File(track.Mp3Url, 0, size));
                }
            }
            return files;
        }

        /// <summary>
        /// Displays the specified message in the log textbox.
        /// </summary>
        /// <param name="message">The message.</param>
        private void Log(String message, Brush color) {
            this.Dispatcher.Invoke(new Action(() => {
                // Time
                var textRange = new TextRange(richTextBoxLog.Document.ContentEnd,
                    richTextBoxLog.Document.ContentEnd);
                textRange.Text = DateTime.Now.ToString("HH:mm:ss") + " ";
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
                // Message
                textRange = new TextRange(richTextBoxLog.Document.ContentEnd,
                    richTextBoxLog.Document.ContentEnd);
                textRange.Text = message;
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                // Line break
                richTextBoxLog.ScrollToEnd();
                richTextBoxLog.AppendText(Environment.NewLine);
            }));
        }

        /// <summary>
        /// Updates the state of the controls.
        /// </summary>
        /// <param name="downloadStarted">True if the download just started, false if it just
        /// stopped.</param>
        private void UpdateControlsState(Boolean downloadStarted) {
            this.Dispatcher.Invoke(new Action(() => {
                if (downloadStarted) {
                    // We just started the download
                    richTextBoxLog.Document.Blocks.Clear();
                    labelProgress.Content = "";
                    progressBar.IsIndeterminate = true;
                    progressBar.Value = progressBar.Minimum;
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
                } else {
                    // We just finished the download (or user has cancelled)
                    buttonStart.IsEnabled = true;
                    buttonStop.IsEnabled = false;
                    buttonBrowse.IsEnabled = true;
                    textBoxUrls.IsReadOnly = false;
                    progressBar.Foreground = new SolidColorBrush(
                        (Color) ColorConverter.ConvertFromString("#FF01D328")); // Green
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = progressBar.Minimum;
                    textBoxDownloadsLocation.IsReadOnly = false;
                    checkBoxCoverArtInFolder.IsEnabled = true;
                    checkBoxCoverArtInTags.IsEnabled = true;
                    checkBoxTag.IsEnabled = true;
                    checkBoxOneAlbumAtATime.IsEnabled = true;
                    checkBoxForceAlbumsDownload.IsEnabled = true;
                    labelDownloadSpeed.Content = "";
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
                File currentFile = this.filesDownload.Where(f => f.Url == fileUrl).First();
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
                        labelDownloadSpeed.Content = ( bytesPerSecond / 1024 ).ToString("0.0") +
                            " kB/s";
                    }));
                }

                // Update UI
                this.Dispatcher.Invoke(new Action(() => {
                    if (!this.userCancelled) {
                        // Update progress label
                        labelProgress.Content =
                            ( (Double) totalReceivedBytes / ( 1024 * 1024 ) ).ToString("0.00") +
                            " MB / " +
                            ( (Double) bytesToDownload / ( 1024 * 1024 ) ).ToString("0.00") +
                            " MB";
                        // Update progress bar
                        progressBar.Value = totalReceivedBytes;
                    }
                }));
            }
        }

        #endregion Methods

        #region Events
        private void buttonBrowse_Click(object sender, RoutedEventArgs e) {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "Select the folder to save albums";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                textBoxDownloadsLocation.Text = dialog.SelectedPath;
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e) {
            if (textBoxUrls.Text == Constants.UrlsHint) {
                // No URL to look
                Log("Paste some albums URLs to be downloaded", Brushes.Red);
                return;
            }

            this.userCancelled = false;

            // Get options
            Boolean tagTracks = checkBoxTag.IsChecked.Value;
            Boolean saveCoverArtInTags = checkBoxCoverArtInTags.IsChecked.Value;
            Boolean saveCoverArtInFolder = checkBoxCoverArtInFolder.IsChecked.Value;
            Boolean oneAlbumAtATime = checkBoxOneAlbumAtATime.IsChecked.Value;
            Boolean forceAlbumsDownload = checkBoxForceAlbumsDownload.IsChecked.Value;
            String downloadsFolder = textBoxDownloadsLocation.Text;
            this.pendingDownloads = new List<WebClient>();

            // Set controls to "downloading..." state
            UpdateControlsState(true);

            Log("Starting download...", Brushes.Black);

            // Get user inputs
            List<String> userUrls = textBoxUrls.Text.Split(new String[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries).ToList();
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
                this.filesDownload = GetFilesToDownload(albums);
            }).ContinueWith(x => {
                // Set progressBar max value
                long bytesToDownload = this.filesDownload.Sum(f => f.Size);
                if (bytesToDownload > 0) {
                    this.Dispatcher.Invoke(new Action(() => {
                        progressBar.IsIndeterminate = false;
                        progressBar.Maximum = bytesToDownload;
                    }));
                }
            }).ContinueWith(x => {
                // Start downloading albums
                if (oneAlbumAtATime) {
                    // Download one album at a time
                    foreach (Album album in albums) {
                        DownloadAlbum(album, downloadsFolder, tagTracks, saveCoverArtInTags,
                            saveCoverArtInFolder);
                    }
                } else {
                    // Parallel download
                    Task[] tasks = new Task[albums.Count];
                    for (int i = 0; i < albums.Count; i++) {
                        Album album = albums[i]; // Mandatory or else => race condition
                        tasks[i] = Task.Factory.StartNew(() =>
                            DownloadAlbum(album, downloadsFolder, tagTracks, saveCoverArtInTags,
                                saveCoverArtInFolder));
                    }
                    // Wait for all albums to be downloaded
                    Task.WaitAll(tasks);
                }
            }).ContinueWith(x => {
                if (this.userCancelled) {
                    // Display message if user cancelled
                    Log("Downloads cancelled by user", Brushes.Black);
                }
            }).ContinueWith(x =>
                // Set controls to "ready" state
                UpdateControlsState(false));
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e) {
            Log("Cancelling downloads. Please wait...", Brushes.Black);
            buttonStop.IsEnabled = false;
            progressBar.Foreground = System.Windows.Media.Brushes.Red;
            progressBar.IsIndeterminate = true;
            lock (this.pendingDownloads) {
                this.userCancelled = true;
                // Stop current downloads
                foreach (WebClient webClient in this.pendingDownloads) {
                    webClient.CancelAsync();
                }
            }
        }

        private void labelAbout_MouseDown(object sender, MouseButtonEventArgs e) {
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

        #endregion Events
    }
}