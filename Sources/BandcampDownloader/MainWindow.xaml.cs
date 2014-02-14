using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace BandcampDownloader {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window {

        // The files to download, or being downloaded, or downloaded
        // Used to compute the current received bytes and the total bytes to download
        private List<File> filesToDownload;

        // Used when user clicks on 'Stop' to abort all current downloads
        private List<WebClient> pendingDownloads;

        // Used when user clicks on 'Stop' to manage the cancelation (UI...)
        private Boolean userCancelled;

        // Used to compute and display the download speed
        private long lastTotalReceivedBytes = 0;
        private DateTime lastDownloadSpeedUpdate;

        #region Constructor
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
                        Log("Downloaded artwork for album \"" + album.Title + "\"");
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
            var artwork = new TagLib.Picture(artworkPath);

            // Create directory to place track files
            String directoryPath = downloadsFolder + "\\" + album.Title.ToAllowedFileName() + "\\";
            try {
                Directory.CreateDirectory(directoryPath);
            } catch {
                Log("ERROR: An error occured when creating the album folder. Make sure you have " +
                    "the rights to write file in the folder you chose");
            }

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
                Log("Finished downloading album \"" + album.Title + "\"");
            }
        }

        private void DownloadAndTagTrack(Album album, TagLib.Picture artwork,
            String albumDirectoryPath, Track track, Boolean tagTracks, Boolean saveCoverArtInTags) {
            // Set location to save the file
            String trackPath = albumDirectoryPath + track.GetFileName(album.Artist);

            var doneEvent = new AutoResetEvent(false);

            using (var webClient = new WebClient()) {
                // Update progress bar when downloading
                webClient.DownloadProgressChanged += (s, e) => {
                    UpdateProgress(track.Mp3Url, e.BytesReceived);
                };

                webClient.DownloadFileCompleted += (s, e) => {
                    if (!e.Cancelled) {
                        if (tagTracks) {
                            // Tag (ID3V2) the file when downloaded
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
                            "\" from album \"" + album.Title + "\"");
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
                    webClient.DownloadFileAsync(new Uri(track.Mp3Url), trackPath);
                }
            }

            // Wait for download to be finished
            doneEvent.WaitOne();
        }

        private List<Album> GetAlbums(List<String> urls) {
            var albums = new List<Album>();

            foreach (String url in urls) {
                // Retrieve URL HTML source code
                String htmlCode = "";
                using (var webClient = new WebClient()) {
                    try {
                        htmlCode = webClient.DownloadString(url);
                    } catch {
                        Log("ERROR: could not retrieve album info for the following URL: " + url);
                        continue;
                    }
                }

                // Get info on album
                try {
                    albums.Add(BandcampHelper.GetAlbum(htmlCode));
                } catch {
                    Log("ERROR: could not retrieve album info for the following URL: " + url);
                    continue;
                }
            }

            return albums;
        }

        private long GetFileSize(String url, String protocolMethod) {
            var webRequest = HttpWebRequest.Create(url);
            webRequest.Method = protocolMethod;
            long fileSize;
            using (WebResponse webResponse = webRequest.GetResponse()) {
                fileSize = webResponse.ContentLength;
            }
            return fileSize;
        }

        private List<File> GetFilesToDownload(List<Album> albums) {
            var files = new List<File>();
            foreach (Album album in albums) {
                // Artwork
                files.Add(new File(album.ArtworkUrl, 0, GetFileSize(album.ArtworkUrl, "HEAD")));

                // Tracks
                foreach (Track track in album.Tracks) {
                    // Using the HEAD method on tracks urls does not work
                    // (Error 405: Method not allowed)
                    // Surprisingly, using the GET method does not seem to download the whole file,
                    // so we will use it to retrieve the mp3 sizes
                    files.Add(new File(track.Mp3Url, 0, GetFileSize(track.Mp3Url, "GET")));
                }
            }
            return files;
        }

        private void Log(String message) {
            this.Dispatcher.Invoke(new Action(() => {
                textBoxLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + " " + message +
                    Environment.NewLine);
                textBoxLog.Focus();
                textBoxLog.CaretIndex = textBoxLog.Text.Length;
                textBoxLog.ScrollToEnd();
            }));
        }

        private void UpdateControlsState(Boolean downloadStarted) {
            this.Dispatcher.Invoke(new Action(() => {
                if (downloadStarted) {
                    // We just started the download
                    textBoxLog.Text = "";
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
                } else {
                    // We just finished the download (or user has cancelled)
                    buttonStart.IsEnabled = true;
                    buttonStop.IsEnabled = false;
                    buttonBrowse.IsEnabled = true;
                    textBoxUrls.IsReadOnly = false;
                    progressBar.Foreground = new SolidColorBrush(
                        (Color) ColorConverter.ConvertFromString("#FF01D328"));
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = progressBar.Minimum;
                    textBoxDownloadsLocation.IsReadOnly = false;
                    checkBoxCoverArtInFolder.IsEnabled = true;
                    checkBoxCoverArtInTags.IsEnabled = true;
                    checkBoxTag.IsEnabled = true;
                    checkBoxOneAlbumAtATime.IsEnabled = true;
                    labelDownloadSpeed.Content = "";
                }
            }));
        }

        private void UpdateProgress(String fileUrl, long bytesReceived) {
            DateTime now = DateTime.Now;

            lock (this.filesToDownload) {
                // Compute new progress values
                File currentFile = this.filesToDownload.Where(f => f.Url == fileUrl).First();
                currentFile.BytesReceived = bytesReceived;
                long totalReceivedBytes = this.filesToDownload.Sum(f => f.BytesReceived);
                long bytesToDownload = this.filesToDownload.Sum(f => f.Size);

                Double bytesPerSecond;
                if (this.lastTotalReceivedBytes == 0) {
                    // First time we update the progress
                    bytesPerSecond = 0;
                    this.lastTotalReceivedBytes = totalReceivedBytes;
                    this.lastDownloadSpeedUpdate = now;

                } else if (( now - this.lastDownloadSpeedUpdate ).TotalMilliseconds > 500) {
                    // Last update of progress happened more than 500 milliseconds ago
                    bytesPerSecond = ((Double) (totalReceivedBytes - this.lastTotalReceivedBytes)) /
                        ( now - this.lastDownloadSpeedUpdate ).TotalSeconds;
                    this.lastTotalReceivedBytes = totalReceivedBytes;
                    this.lastDownloadSpeedUpdate = now;

                    // Update UI
                    this.Dispatcher.Invoke(new Action(() => {
                        labelDownloadSpeed.Content = (bytesPerSecond / 1024).ToString("0.0") + 
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
                Log("ERROR: Paste some albums URLs to be downloaded");
                return;
            }

            this.userCancelled = false;
            // Get options
            Boolean tagTracks = checkBoxTag.IsChecked.Value;
            Boolean saveCoverArtInTags = checkBoxCoverArtInTags.IsChecked.Value;
            Boolean saveCoverArtInFolder = checkBoxCoverArtInFolder.IsChecked.Value;
            Boolean oneAlbumAtATime = checkBoxOneAlbumAtATime.IsChecked.Value;
            String downloadsFolder = textBoxDownloadsLocation.Text;
            this.pendingDownloads = new List<WebClient>();

            // Set controls to "downloading..." state
            UpdateControlsState(true);

            Log("Starting download...");

            List<String> urls = textBoxUrls.Text.Split(new String[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries).ToList();
            urls = urls.Distinct().ToList();

            var albums = new List<Album>();

            Task.Factory.StartNew(() => {
                // Get info on albums
                albums = GetAlbums(urls);
            }).ContinueWith(x => {
                // Save files to download (we'll need the list to update the progressBar)
                this.filesToDownload = GetFilesToDownload(albums);
            }).ContinueWith(x => {
                // Set progressBar max value
                long bytesToDownload = this.filesToDownload.Sum(f => f.Size);
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
                    Log("Downloads cancelled by user");
                }
            }).ContinueWith(x =>
                // Set controls to "ready" state
                UpdateControlsState(false));
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e) {
            Log("Cancelling downloads. Please wait...");
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