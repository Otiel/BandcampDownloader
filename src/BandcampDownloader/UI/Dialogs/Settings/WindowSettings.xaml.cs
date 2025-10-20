using System.IO;
using System.Windows;
using BandcampDownloader.Core;
using Config.Net;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class WindowSettings
{
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
        ActiveDownloads = activeDownloads; // Must be done before UI initialization
        DataContext = App.UserSettings;
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
        Close();
    }

    /// <summary>
    /// Cancels the changes already applied.
    /// </summary>
    private void CancelChanges()
    {
        userControlSettingsAdvanced.CancelChanges();
        userControlSettingsCoverArt.CancelChanges();
        userControlSettingsDownloads.CancelChanges();
        userControlSettingsGeneral.CancelChanges();
        userControlSettingsNetwork.CancelChanges();
        userControlSettingsPlaylist.CancelChanges();
        userControlSettingsTags.CancelChanges();
    }

    /// <summary>
    /// Resets settings to their default values.
    /// </summary>
    private void ResetSettings()
    {
        // Save settings we shouldn't reset (as they're not on the Settings window)
        var downloadsPath = App.UserSettings.DownloadsPath;
        var downloadArtistDiscography = App.UserSettings.DownloadArtistDiscography;

        File.Delete(Constants.UserSettingsFilePath);
        App.UserSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();

        // Load back settings we shouldn't reset
        App.UserSettings.DownloadsPath = downloadsPath;
        App.UserSettings.DownloadArtistDiscography = downloadArtistDiscography;

        // Re-load settings on UI
        userControlSettingsAdvanced.LoadSettings();
        userControlSettingsCoverArt.LoadSettings();
        userControlSettingsDownloads.LoadSettings();
        userControlSettingsGeneral.LoadSettings();
        userControlSettingsNetwork.LoadSettings();
        userControlSettingsPlaylist.LoadSettings();
        userControlSettingsTags.LoadSettings();
    }

    /// <summary>
    /// Saves all settings.
    /// </summary>
    private void SaveSettings()
    {
        userControlSettingsAdvanced.SaveSettings();
        userControlSettingsCoverArt.SaveSettings();
        userControlSettingsDownloads.SaveSettings();
        userControlSettingsGeneral.SaveSettings();
        userControlSettingsNetwork.SaveSettings();
        userControlSettingsPlaylist.SaveSettings();
        userControlSettingsTags.SaveSettings();
    }
}
