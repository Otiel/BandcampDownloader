using System;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using BandcampDownloader.Core;
using BandcampDownloader.Helpers;
using BandcampDownloader.Logging;
using BandcampDownloader.Settings;
using BandcampDownloader.UI.Dialogs.Settings;
using BandcampDownloader.UI.Dialogs.Update;
using Microsoft.Win32;
using NLog;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs;

internal sealed partial class WindowMain
{
    private readonly IUserSettings _userSettings;

    /// <summary>
    /// True if there are active downloads; false otherwise.
    /// </summary>
    private bool _activeDownloads;

    /// <summary>
    /// The DownloadManager used to download albums.
    /// </summary>
    private DownloadManager _downloadManager;

    /// <summary>
    /// Used to compute and display the download speed.
    /// </summary>
    private DateTime _lastDownloadSpeedUpdate;

    /// <summary>
    /// Used to compute and display the download speed.
    /// </summary>
    private long _lastTotalReceivedBytes;

    /// <summary>
    /// Used when user clicks on 'Cancel' to manage the cancellation (UI...).
    /// </summary>
    private bool _userCancelled;

    public WindowMain(ISettingsService settingsService)
    {
        _userSettings = settingsService.GetUserSettings();

        // Save DataContext for bindings (must be called before initializing UI)
        DataContext = _userSettings;

        InitializeComponent();

#if DEBUG
        TextBoxUrls.Text = ""
                           //+ "https://projectmooncircle.bandcamp.com" /* Lots of albums (124) */ + Environment.NewLine
                           //+ "https://goataholicskjald.bandcamp.com/album/dogma" /* #65 Downloaded size ≠ predicted */ + Environment.NewLine
                           //+ "https://mstrvlk.bandcamp.com/album/-" /* #64 Album with big cover */ + Environment.NewLine
                           //+ "https://mstrvlk.bandcamp.com/track/-" /* #64 Track with big cover */ + Environment.NewLine
                           //+ "https://weneverlearnedtolive.bandcamp.com/album/silently-i-threw-them-skyward" /* #42 Album with lyrics */ + Environment.NewLine
                           //+ "https://weneverlearnedtolive.bandcamp.com/track/shadows-in-hibernation-2" /* #42 Track with lyrics */ + Environment.NewLine
                           //+ "https://goataholicskjald.bandcamp.com/track/europa" + Environment.NewLine
                           //+ "https://goataholicskjald.bandcamp.com/track/epilogue" + Environment.NewLine
                           //+ "https://afterdarkrecordings.bandcamp.com/album/adr-unreleased-tracks" /* #69 Album without cover */ + Environment.NewLine
                           //+ "https://liluglymane.bandcamp.com/album/study-of-the-hypothesized-removable-and-or-expandable-nature-of-human-capability-and-limitations-primarily-regarding-introductory-experiences-with-new-and-exciting-technologies-by-way-of-motivati-2" /* #54 Long path */ + Environment.NewLine
                           //+ "https://brzoskamarciniakmarkiewicz.bandcamp.com/album/wp-aw" /* #82 Tracks with diacritics */ + Environment.NewLine
                           //+ "https://empyrium.bandcamp.com/album/der-wie-ein-blitz-vom-himmel-fiel" /* #102 Album ending with '...' */ + Environment.NewLine
                           + "https://tympanikaudio.bandcamp.com" /* #118 Different discography page */ + Environment.NewLine
            ;
#endif
    }

    private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        var dialogResult = dialog.ShowDialog();
        if (dialogResult is true)
        {
            TextBoxDownloadsPath.Text = dialog.FolderName + "\\{artist}\\{album}";
            // Force update of the settings file (it's not done unless the user gives then loses focus on the textbox)
            TextBoxDownloadsPath.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }

    private void ButtonOpenSettingsWindow_Click(object sender, RoutedEventArgs e)
    {
        var windowSettings = new WindowSettings(_activeDownloads)
        {
            Owner = this,
            ShowInTaskbar = false,
        };
        windowSettings.ShowDialog();
    }

    private async void ButtonStart_Click(object sender, RoutedEventArgs e)
    {
        if (TextBoxUrls.Text == "")
        {
            // No URL to look
            Log("Paste some albums URLs to be downloaded", LogType.Error);
            return;
        }

        // Set controls to "downloading..." state
        _activeDownloads = true;
        UpdateControlsState(true);

        Log("Starting download...", LogType.Info);

        await StartDownloadAsync();

        if (_userCancelled)
        {
            // Display message if user cancelled
            Log("Downloads cancelled by user", LogType.Info);
        }

        // Reset controls to "ready" state
        _activeDownloads = false;
        _lastTotalReceivedBytes = 0;
        UpdateControlsState(false);
        Mouse.OverrideCursor = null;

        if (_userSettings.EnableApplicationSounds)
        {
            // Play a sound
            try
            {
                using (var soundPlayer = new SoundPlayer(@"C:\Windows\Media\Windows Ding.wav"))
                {
                    soundPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                Log("Could not play 'finished' sound", LogType.Error);
                Log(ex.ToString(), LogType.VerboseInfo);
            }
        }
    }

    private void ButtonStop_Click(object sender, RoutedEventArgs e)
    {
        var msgProperties = new WpfMessageBoxProperties
        {
            Button = MessageBoxButton.YesNo,
            ButtonCancelText = Properties.Resources.messageBoxButtonCancel,
            ButtonOkText = Properties.Resources.messageBoxButtonOK,
            Image = MessageBoxImage.Question,
            Text = Properties.Resources.messageBoxCancelDownloadsText,
            Title = "Bandcamp Downloader",
        };

        if (WpfMessageBox.Show(this, ref msgProperties) != MessageBoxResult.Yes || !_activeDownloads)
        {
            // If user cancelled the cancellation or if downloads finished while he choosed to cancel
            return;
        }

        Mouse.OverrideCursor = Cursors.Wait;
        _userCancelled = true;
        ButtonStop.IsEnabled = false;

        _downloadManager.CancelDownloads();
    }

    /// <summary>
    /// Displays a message if a new version is available.
    /// </summary>
    private async Task CheckForUpdates()
    {
        Version latestVersion;
        try
        {
            latestVersion = await UpdatesHelper.GetLatestVersionAsync();
        }
        catch (CouldNotCheckForUpdatesException)
        {
            LabelNewVersion.Content = Properties.Resources.labelVersionError;
            LabelNewVersion.Visibility = Visibility.Visible;
            return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (currentVersion!.CompareTo(latestVersion) < 0)
        {
            // The latest version is newer than the current one
            LabelNewVersion.Content = Properties.Resources.labelVersionNewUpdateAvailable;
            LabelNewVersion.Visibility = Visibility.Visible;
        }
    }

    private void DownloadManager_LogAdded(object sender, LogArgs eventArgs)
    {
        Log(eventArgs.Message, eventArgs.LogType);
    }

    private void LabelNewVersion_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var windowUpdate = new WindowUpdate
        {
            Owner = this,
            ShowInTaskbar = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };
        windowUpdate.Show();
    }

    /// <summary>
    /// Logs to file and displays the specified message in the log textbox.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="logType">The log type.</param>
    private void Log(string message, LogType logType)
    {
        // Log to file
        var logger = LogManager.GetCurrentClassLogger();
        logger.Log(logType.ToNLogLevel(), message);

        // Log to window
        if (_userSettings.ShowVerboseLog || logType == LogType.Error || logType == LogType.Info || logType == LogType.IntermediateSuccess || logType == LogType.Success)
        {
            // Time
            var textRange = new TextRange(RichTextBoxLog.Document.ContentEnd, RichTextBoxLog.Document.ContentEnd)
            {
                Text = DateTime.Now.ToString("HH:mm:ss") + " ",
            };
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
            // Message
            textRange = new TextRange(RichTextBoxLog.Document.ContentEnd, RichTextBoxLog.Document.ContentEnd)
            {
                Text = message,
            };
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, LogHelper.GetColor(logType));
            // Line break
            RichTextBoxLog.AppendText(Environment.NewLine);

            if (RichTextBoxLog.IsScrolledToEnd())
            {
                RichTextBoxLog.ScrollToEnd();
            }
        }
    }

    /// <summary>
    /// Starts downloads.
    /// </summary>
    private async Task StartDownloadAsync()
    {
        _userCancelled = false;

        // Initializes the DownloadManager
        _downloadManager = new DownloadManager(TextBoxUrls.Text);
        _downloadManager.LogAdded += DownloadManager_LogAdded;

        // Fetch URL to get the files size
        await _downloadManager.FetchUrlsAsync();

        // Set progressBar max value
        long maxProgressBarValue;
        if (_userSettings.RetrieveFilesSize)
        {
            maxProgressBarValue = _downloadManager.DownloadingFiles.Sum(f => f.Size); // Bytes to download
        }
        else
        {
            maxProgressBarValue = _downloadManager.DownloadingFiles.Count; // Number of files to download
        }

        if (maxProgressBarValue > 0)
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Maximum = maxProgressBarValue;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
        }

        // Start timer to update progress on UI
        var updateProgressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100),
        };
        updateProgressTimer.Tick += UpdateProgressTimer_Tick;
        updateProgressTimer.Start();

        // Start timer to update download speed on UI
        var updateDownloadSpeedTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000),
        };
        updateDownloadSpeedTimer.Tick += UpdateDownloadSpeedTimer_Tick;
        updateDownloadSpeedTimer.Start();

        // Start downloading albums
        await _downloadManager.StartDownloadsAsync();

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
    private void UpdateControlsState(bool downloadStarted)
    {
        if (downloadStarted)
        {
            // We just started the download
            ButtonBrowse.IsEnabled = false;
            ButtonStart.IsEnabled = false;
            ButtonStop.IsEnabled = true;
            CheckBoxDownloadDiscography.IsEnabled = false;
            LabelProgress.Content = "";
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Value = ProgressBar.Minimum;
            RichTextBoxLog.Document.Blocks.Clear();
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            TaskbarItemInfo.ProgressValue = 0;
            TextBoxDownloadsPath.IsReadOnly = true;
            TextBoxUrls.IsReadOnly = true;
        }
        else
        {
            // We just finished the download (or user has cancelled)
            ButtonBrowse.IsEnabled = true;
            ButtonStart.IsEnabled = true;
            ButtonStop.IsEnabled = false;
            CheckBoxDownloadDiscography.IsEnabled = true;
            LabelDownloadSpeed.Content = "";
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = ProgressBar.Minimum;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            TaskbarItemInfo.ProgressValue = 0;
            TextBoxDownloadsPath.IsReadOnly = false;
            TextBoxUrls.IsReadOnly = false;
        }
    }

    /// <summary>
    /// Updates the download speed on UI.
    /// </summary>
    private void UpdateDownloadSpeed()
    {
        var now = DateTime.Now;

        // Compute new progress values
        var totalReceivedBytes = _downloadManager.DownloadingFiles.Sum(f => f.BytesReceived);

        var bytesPerSecond =
            (totalReceivedBytes - _lastTotalReceivedBytes) /
            (now - _lastDownloadSpeedUpdate).TotalSeconds;
        _lastTotalReceivedBytes = totalReceivedBytes;
        _lastDownloadSpeedUpdate = now;

        // Update download speed on UI
        LabelDownloadSpeed.Content = (bytesPerSecond / 1024).ToString("0.0") + " kB/s";
    }

    private void UpdateDownloadSpeedTimer_Tick(object sender, EventArgs e)
    {
        UpdateDownloadSpeed();
    }

    /// <summary>
    /// Updates the progress label on UI.
    /// </summary>
    private void UpdateProgress()
    {
        // Compute new progress values
        var totalReceivedBytes = _downloadManager.DownloadingFiles.Sum(f => f.BytesReceived);
        var bytesToDownload = _downloadManager.DownloadingFiles.Sum(f => f.Size);

        // Update progress label
        LabelProgress.Content =
            ((double)totalReceivedBytes / (1024 * 1024)).ToString("0.00") + " MB" +
            (_userSettings.RetrieveFilesSize ? " / " + ((double)bytesToDownload / (1024 * 1024)).ToString("0.00") + " MB" : "");

        if (_userSettings.RetrieveFilesSize)
        {
            // Update progress bar based on bytes received
            ProgressBar.Value = totalReceivedBytes;
            // Taskbar progress is between 0 and 1
            TaskbarItemInfo.ProgressValue = totalReceivedBytes / ProgressBar.Maximum;
        }
        else
        {
            double downloadedFilesCount = _downloadManager.DownloadingFiles.Count(f => f.Downloaded);
            // Update progress bar based on downloaded files
            ProgressBar.Value = downloadedFilesCount;
            // Taskbar progress is between 0 and count of files to download
            TaskbarItemInfo.ProgressValue = downloadedFilesCount / ProgressBar.Maximum;
        }
    }

    private void UpdateProgressTimer_Tick(object sender, EventArgs e)
    {
        UpdateProgress();
    }

    private void WindowMain_Closing(object sender, CancelEventArgs e)
    {
        if (_activeDownloads)
        {
            // There are active downloads, ask for confirmation
            var msgProperties = new WpfMessageBoxProperties
            {
                Button = MessageBoxButton.OKCancel,
                ButtonCancelText = Properties.Resources.messageBoxButtonCancel,
                ButtonOkText = Properties.Resources.messageBoxCloseWindowWhenDownloadingButtonOk,
                Image = MessageBoxImage.Warning,
                Text = Properties.Resources.messageBoxCloseWindowWhenDownloadingText,
                Title = "Bandcamp Downloader",
            };

            if (WpfMessageBox.Show(this, ref msgProperties) == MessageBoxResult.Cancel)
            {
                // Cancel closing the window
                e.Cancel = true;
            }
        }
    }

    private async void WindowMain_Loaded(object sender, RoutedEventArgs e)
    {
        if (_userSettings.CheckForUpdates)
        {
            await CheckForUpdates();
        }
    }
}
