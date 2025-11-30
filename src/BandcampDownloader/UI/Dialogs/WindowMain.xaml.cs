using System;
using System.ComponentModel;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using BandcampDownloader.Bandcamp.Download;
using BandcampDownloader.Core.Logging;
using BandcampDownloader.Core.Updates;
using BandcampDownloader.Helpers;
using BandcampDownloader.Settings;
using BandcampDownloader.Threading;
using BandcampDownloader.UI.Dialogs.Settings;
using BandcampDownloader.UI.Dialogs.Update;
using Microsoft.Win32;
using NLog;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs;

internal sealed partial class WindowMain
{
    private readonly IUserSettings _userSettings;
    private readonly IDownloadManager _downloadManager;
    private readonly IUpdatesService _updatesService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private CancellationTokenSource _downloadCts;

    /// <summary>
    /// True if there are active downloads; false otherwise.
    /// </summary>
    private bool _activeDownloads;

    /// <summary>
    /// Used to compute and display the download speed.
    /// </summary>
    private DateTime _lastDownloadSpeedUpdate;

    /// <summary>
    /// Used to compute and display the download speed.
    /// </summary>
    private long _lastTotalReceivedBytes;


    public WindowMain(ISettingsService settingsService, IDownloadManager downloadManager, IUpdatesService updatesService)
    {
        _userSettings = settingsService.GetUserSettings();
        _downloadManager = downloadManager;
        _updatesService = updatesService;

        _downloadManager.DownloadProgressChanged += DownloadProgressChanged;

        // Save DataContext for bindings (must be called before initializing UI)
        DataContext = _userSettings;

        InitializeComponent();

#if DEBUG
        TextBoxUrls.Text = ""
                           //+ "https://projectmooncircle.bandcamp.com" /* Lots of albums (124) */ + Environment.NewLine
                           //+ "https://goataholicskjald.bandcamp.com/album/dogma" /* #65 Downloaded size ≠ predicted */ + Environment.NewLine // TODO fix Could not retrieve data for https://goataholicskjald.bandcamp.com/track/europa"><span class="track-title
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
                           //+ "https://tympanikaudio.bandcamp.com" /* #118 Different discography page */ + Environment.NewLine
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
        var inputUrls = TextBoxUrls.Text;

        await Task.Run(async () =>
        {
            _downloadCts = new CancellationTokenSource();

            if (string.IsNullOrWhiteSpace(inputUrls))
            {
                // No URL to look
                await LogAsync("Paste some albums URLs to be downloaded", DownloadProgressChangedLevel.Error);
                return;
            }

            // Set controls to "downloading..." state
            _activeDownloads = true;
            await UpdateControlsStateAsync(true);

            await LogAsync("Starting download...", DownloadProgressChangedLevel.Info);

            try
            {
                await StartDownloadAsync(inputUrls, _downloadCts.Token);
            }
            catch (OperationCanceledException)
            {
                await LogAsync("Downloads cancelled by user", DownloadProgressChangedLevel.Info);
            }

            // Reset controls to "ready" state
            _activeDownloads = false;
            _lastTotalReceivedBytes = 0;
            await UpdateControlsStateAsync(false);
            await ModifyMouseCursorAsync(null);

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
                    await LogAsync("Could not play 'finished' sound", DownloadProgressChangedLevel.Error);
                    await LogAsync(ex.ToString(), DownloadProgressChangedLevel.VerboseInfo);
                }
            }
        });
    }

    private async void ButtonStop_Click(object sender, RoutedEventArgs e)
    {
        var msgProperties = new WpfMessageBoxProperties
        {
            Button = WpfMessageBoxButton.YesNo,
            ButtonCancelText = Properties.Resources.messageBoxButtonCancel,
            ButtonOkText = Properties.Resources.messageBoxButtonOK,
            Image = WpfMessageBoxImage.Question,
            Text = Properties.Resources.messageBoxCancelDownloadsText,
            Title = "Bandcamp Downloader",
        };

        if (WpfMessageBox.Show(this, ref msgProperties) != WpfMessageBoxResult.Yes ||
            !_activeDownloads)
        {
            // If user cancelled the cancellation or if downloads finished while he chose to cancel
            return;
        }

        await ModifyMouseCursorAsync(Cursors.Wait);
        ButtonStop.IsEnabled = false;

        await _downloadCts.CancelAsync();
    }

    private static async Task ModifyMouseCursorAsync(Cursor cursor)
    {
        await ThreadUtils.ExecuteOnUiAsync(() =>
        {
            Mouse.OverrideCursor = cursor;
        });
    }

    /// <summary>
    /// Displays a message if a new version is available.
    /// </summary>
    private async Task CheckForUpdatesAsync()
    {
        Version latestVersion;
        try
        {
            latestVersion = await _updatesService.GetLatestVersionAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get latest version");

            await ShowNewVersionLabelAsync(Properties.Resources.labelVersionError);
            return;
        }

        if (latestVersion.IsNewerVersion())
        {
            await ShowNewVersionLabelAsync(Properties.Resources.labelVersionNewUpdateAvailable);
        }
    }

    private async Task ShowNewVersionLabelAsync(string content)
    {
        await ThreadUtils.ExecuteOnUiAsync(() =>
        {
            LabelNewVersion.Content = content;
            LabelNewVersion.Visibility = Visibility.Visible;
        });
    }

    private async void DownloadProgressChanged(object sender, DownloadProgressChangedArgs eventArgs)
    {
        await LogAsync(eventArgs.Message, eventArgs.Level);
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
    /// <param name="downloadProgressChangedLevel">The log type.</param>
    private async Task LogAsync(string message, DownloadProgressChangedLevel downloadProgressChangedLevel)
    {
        // Log to file
        _logger.Log(downloadProgressChangedLevel.ToNLogLevel(), message);

        // Log to window
        if (_userSettings.ShowVerboseLog || downloadProgressChangedLevel == DownloadProgressChangedLevel.Error || downloadProgressChangedLevel == DownloadProgressChangedLevel.Info || downloadProgressChangedLevel == DownloadProgressChangedLevel.IntermediateSuccess || downloadProgressChangedLevel == DownloadProgressChangedLevel.Success)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss") + " ";

            await ThreadUtils.ExecuteOnUiAsync(() =>
            {
                try
                {
                    // Make changes to RichTextBox in a block to minimize UI freeze
                    RichTextBoxLog.BeginChange();

                    // Time
                    var documentContentEnd = RichTextBoxLog.Document.ContentEnd;
                    var timestampTextRange = new TextRange(documentContentEnd, documentContentEnd)
                    {
                        Text = timestamp,
                    };
                    timestampTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);

                    // Message
                    documentContentEnd = RichTextBoxLog.Document.ContentEnd;
                    var messageTextRange = new TextRange(documentContentEnd, documentContentEnd)
                    {
                        Text = message,
                    };
                    messageTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, LogHelper.GetColor(downloadProgressChangedLevel));

                    // Line break
                    RichTextBoxLog.AppendText(Environment.NewLine);

                    if (RichTextBoxLog.IsScrolledToEnd())
                    {
                        RichTextBoxLog.ScrollToEnd();
                    }
                }
                finally
                {
                    RichTextBoxLog.EndChange();
                }
            });
        }
    }

    /// <summary>
    /// Starts downloads.
    /// </summary>
    private async Task StartDownloadAsync(string inputUrls, CancellationToken cancellationToken)
    {
        await _downloadManager.InitializeAsync(inputUrls, cancellationToken);

        // Set progressBar max value
        var maxProgressBarValue = _userSettings.RetrieveFilesSize ? _downloadManager.GetTotalBytesToDownload() : _downloadManager.GetTotalFilesCountToDownload();

        if (maxProgressBarValue > 0)
        {
            await ThreadUtils.ExecuteOnUiAsync(() =>
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Maximum = maxProgressBarValue;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            });
        }

        var timersCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Start timer to update progress on UI
        var updateProgressTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        // In fire and forget
        _ = Task.Run(async () =>
        {
            while (await updateProgressTimer.WaitForNextTickAsync(timersCts.Token))
            {
                await UpdateProgressAsync();
            }
        }, timersCts.Token);

        // Start timer to update download speed on UI
        var updateDownloadSpeedTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
        // In fire and forget
        _ = Task.Run(async () =>
        {
            while (await updateDownloadSpeedTimer.WaitForNextTickAsync(timersCts.Token))
            {
                await UpdateDownloadSpeedAsync();
            }
        }, timersCts.Token);

        // Start downloading albums
        // ReSharper disable once PossiblyMistakenUseOfCancellationToken : this is the correct token
        await _downloadManager.StartDownloadsAsync(cancellationToken);

        // Stop timers
        await timersCts.CancelAsync();

        // Update progress one last time to make sure the downloaded bytes displayed on UI is up-to-date
        await UpdateProgressAsync();
    }

    /// <summary>
    /// Updates the state of the controls.
    /// </summary>
    /// <param name="downloadStarted">True if the download just started; false if it just stopped.</param>
    private async Task UpdateControlsStateAsync(bool downloadStarted)
    {
        await ThreadUtils.ExecuteOnUiAsync(() =>
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
        });
    }

    /// <summary>
    /// Updates the download speed on UI.
    /// </summary>
    private async Task UpdateDownloadSpeedAsync()
    {
        var now = DateTime.Now;

        // Compute new progress values
        var totalReceivedBytes = _downloadManager.GetTotalBytesReceived();

        var bytesPerSecond = (totalReceivedBytes - _lastTotalReceivedBytes) /
                             (now - _lastDownloadSpeedUpdate).TotalSeconds;
        _lastTotalReceivedBytes = totalReceivedBytes;
        _lastDownloadSpeedUpdate = now;

        var downloadSpeedLabelContent = (bytesPerSecond / 1024).ToString("0.0") + " kB/s";

        await ThreadUtils.ExecuteOnUiAsync(() =>
        {
            // Update download speed on UI
            LabelDownloadSpeed.Content = downloadSpeedLabelContent;
        });
    }

    /// <summary>
    /// Updates the progress label on UI.
    /// </summary>
    private async Task UpdateProgressAsync()
    {
        // Compute new progress values
        var totalReceivedBytes = _downloadManager.GetTotalBytesReceived();
        var bytesToDownload = _downloadManager.GetTotalBytesToDownload();

        // Compute progress
        var labelProgressContent = (totalReceivedBytes / (1024 * 1024)).ToString("0.00") + " MB" +
                                   (_userSettings.RetrieveFilesSize ? " / " + (bytesToDownload / (1024 * 1024)).ToString("0.00") + " MB" : "");

        var progressBarMaximum = await ThreadUtils.ExecuteOnUiAsync(() => ProgressBar.Maximum);
        double progressBarValue;
        double taskbarProgressBarValue;
        if (_userSettings.RetrieveFilesSize)
        {
            // Update progress bar based on bytes received
            progressBarValue = totalReceivedBytes;
            // Taskbar progress is between 0 and 1
            taskbarProgressBarValue = totalReceivedBytes / progressBarMaximum;
        }
        else
        {
            var downloadedFilesCount = _downloadManager.GetTotalFilesCountReceived();
            // Update progress bar based on downloaded files
            progressBarValue = downloadedFilesCount;
            // Taskbar progress is between 0 and count of files to download
            taskbarProgressBarValue = downloadedFilesCount / progressBarMaximum;
        }

        await ThreadUtils.ExecuteOnUiAsync(() =>
        {
            LabelProgress.Content = labelProgressContent;
            ProgressBar.Value = progressBarValue;
            TaskbarItemInfo.ProgressValue = taskbarProgressBarValue;
        });
    }

    private void WindowMain_Closing(object sender, CancelEventArgs e)
    {
        if (_activeDownloads)
        {
            // There are active downloads, ask for confirmation
            var msgProperties = new WpfMessageBoxProperties
            {
                Button = WpfMessageBoxButton.OKCancel,
                ButtonCancelText = Properties.Resources.messageBoxButtonCancel,
                ButtonOkText = Properties.Resources.messageBoxCloseWindowWhenDownloadingButtonOk,
                Image = WpfMessageBoxImage.Exclamation,
                Text = Properties.Resources.messageBoxCloseWindowWhenDownloadingText,
                Title = "Bandcamp Downloader",
            };

            if (WpfMessageBox.Show(this, ref msgProperties) == WpfMessageBoxResult.Cancel)
            {
                // Cancel closing the window
                e.Cancel = true;
            }
        }
    }

    private void WindowMain_Loaded(object sender, RoutedEventArgs e)
    {
        // In fire and forget in order to prevent delaying the window access
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
#pragma warning restore CS4014
        {
            if (_userSettings.CheckForUpdates)
            {
                await CheckForUpdatesAsync();
            }
        });
    }
}
