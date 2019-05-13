using System.IO;
using System.Windows;
using Config.Net;

namespace BandcampDownloader {

    public partial class WindowSettings: Window {

        /// <summary>
        /// True if there are active downloads; false otherwise.
        /// </summary>
        public bool ActiveDownloads { get; set; }

        /// <summary>
        /// Creates a new instance of SettingsWindow.
        /// </summary>
        /// <param name="activeDownloads">True if there are active downloads; false otherwise.</param>
        public WindowSettings(bool activeDownloads) {
            ActiveDownloads = activeDownloads; // Must be done before UI initialization
            DataContext = App.UserSettings;
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonResetSettings_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(Properties.Resources.messageBoxResetSettings, "Bandcamp Downloader", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                ResetSettings();
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e) {
            SaveSettings();
            Close();
        }

        /// <summary>
        /// Resets settings to their default values.
        /// </summary>
        private void ResetSettings() {
            // Save settings we shouldn't reset (as they're not on the Settings window)
            string downloadsPath = App.UserSettings.DownloadsPath;
            bool downloadArtistDiscography = App.UserSettings.DownloadArtistDiscography;

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
        /// Save all settings.
        /// </summary>
        private void SaveSettings() {
            userControlSettingsAdvanced.SaveSettings();
            userControlSettingsCoverArt.SaveSettings();
            userControlSettingsDownloads.SaveSettings();
            userControlSettingsGeneral.SaveSettings();
            userControlSettingsNetwork.SaveSettings();
            userControlSettingsPlaylist.SaveSettings();
            userControlSettingsTags.SaveSettings();
        }
    }
}