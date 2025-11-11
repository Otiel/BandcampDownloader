using System.Windows;
using BandcampDownloader.Core.DependencyInjection;
using BandcampDownloader.Settings;
using NLog;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class WindowSettings
{
    private readonly ISettingsService _settingsService;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// True if there are active downloads; false otherwise.
    /// </summary>
    public bool ActiveDownloads { get; set; }

    /// <summary>
    /// Creates a new instance of SettingsWindow.
    /// </summary>
    /// <param name="activeDownloads">True if there are active downloads; false otherwise.</param>
    public WindowSettings(bool activeDownloads)
    {
        _settingsService = DependencyInjectionHelper.GetService<ISettingsService>();

        ActiveDownloads = activeDownloads; // Must be done before UI initialization
        InitializeComponent();
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        CancelChanges();
        Close();
    }

    private void ButtonResetSettings_Click(object sender, RoutedEventArgs e)
    {
        var msgProperties = new WpfMessageBoxProperties
        {
            Button = MessageBoxButton.OKCancel,
            ButtonOkText = Properties.Resources.messageBoxResetSettingsButtonOk,
            ButtonCancelText = Properties.Resources.messageBoxButtonCancel,
            Image = MessageBoxImage.Question,
            Text = Properties.Resources.messageBoxResetSettingsText,
            Title = "Bandcamp Downloader",
        };

        if (WpfMessageBox.Show(this, ref msgProperties) == MessageBoxResult.OK)
        {
            ResetSettings();
        }
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        LogSettings();
        Close();
    }

    /// <summary>
    /// Cancels the changes already applied.
    /// </summary>
    private void CancelChanges()
    {
        UserControlSettingsAdvanced.CancelChanges();
        UserControlSettingsCoverArt.CancelChanges();
        UserControlSettingsDownloads.CancelChanges();
        UserControlSettingsGeneral.CancelChanges();
        UserControlSettingsNetwork.CancelChanges();
        UserControlSettingsPlaylist.CancelChanges();
        UserControlSettingsTags.CancelChanges();
    }

    /// <summary>
    /// Resets settings to their default values.
    /// </summary>
    private void ResetSettings()
    {
        var userSettings = _settingsService.GetUserSettings();

        // Save settings we shouldn't reset (as they're not on the Settings window)
        var downloadsPath = userSettings.DownloadsPath;
        var downloadArtistDiscography = userSettings.DownloadArtistDiscography;

        // Reset settings
        userSettings = _settingsService.ResetSettings();

        // Load back settings we shouldn't reset
        userSettings.DownloadsPath = downloadsPath;
        userSettings.DownloadArtistDiscography = downloadArtistDiscography;

        // Re-load settings on UI
        UserControlSettingsAdvanced.LoadSettings(userSettings);
        UserControlSettingsCoverArt.LoadSettings(userSettings);
        UserControlSettingsDownloads.LoadSettings(userSettings);
        UserControlSettingsGeneral.LoadSettings(userSettings);
        UserControlSettingsNetwork.LoadSettings(userSettings);
        UserControlSettingsPlaylist.LoadSettings(userSettings);
        UserControlSettingsTags.LoadSettings(userSettings);
    }

    /// <summary>
    /// Saves all settings.
    /// </summary>
    private void SaveSettings()
    {
        UserControlSettingsAdvanced.SaveSettings();
        UserControlSettingsCoverArt.SaveSettings();
        UserControlSettingsDownloads.SaveSettings();
        UserControlSettingsGeneral.SaveSettings();
        UserControlSettingsNetwork.SaveSettings();
        UserControlSettingsPlaylist.SaveSettings();
        UserControlSettingsTags.SaveSettings();
    }

    private void LogSettings()
    {
        var userSettingsJson = _settingsService.GetUserSettingsInJson();
        _logger.Info($"Settings saved: {userSettingsJson}");
    }
}
