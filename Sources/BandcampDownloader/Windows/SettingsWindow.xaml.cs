using System;
using System.IO;
using System.Windows;
using Config.Net;

namespace BandcampDownloader {

    public partial class SettingsWindow: Window {

        /// <summary>
        /// True if there are active downloads; false otherwise.
        /// </summary>
        public Boolean ActiveDownloads { get; set; }

        /// <summary>
        /// Creates a new instance of SettingsWindow.
        /// </summary>
        /// <param name="activeDownloads">True if there are active downloads; false otherwise.</param>
        public SettingsWindow(Boolean activeDownloads) {
            ActiveDownloads = activeDownloads; // Must be done before UI initialization
            DataContext = MainWindow.userSettings;
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonResetSettings_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Reset settings to their default values?", "Bandcamp Downloader", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
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
            String downloadsPath = MainWindow.userSettings.DownloadsLocation;

            File.Delete(Constants.UserSettingsFilePath);
            MainWindow.userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
            MainWindow.userSettings.DownloadsLocation = downloadsPath;
            DataContext = MainWindow.userSettings;
        }

        /// <summary>
        /// Save all settings.
        /// </summary>
        private void SaveSettings() {
            userControlSettingsTags.SaveSettings();
            userControlSettingsCoverArt.SaveSettings();
            userControlSettingsLog.SaveSettings();
            userControlSettingsAdvanced.SaveSettings();
        }
    }
}