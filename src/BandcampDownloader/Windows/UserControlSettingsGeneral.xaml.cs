using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsGeneral: UserControl {

        public UserControlSettingsGeneral() {
            InitializeComponent();
            // Save data context for bindings
            DataContext = App.UserSettings;
        }

        /// <summary>
        /// Loads settings from App.UserSettings.
        /// </summary>
        public void LoadSettings() {
            // Reload DataContext in case settings have changed
            DataContext = App.UserSettings;
            // No need to call UpdateTarget, it is done automatically
        }

        /// <summary>
        /// Saves settings to App.UserSettings.
        /// </summary>
        public void SaveSettings() {
            checkBoxCheckForUpdates.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxVerboseLog.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        }

        private void ButtonCheckForUpdates_Click(object sender, RoutedEventArgs e) {
            Version latestVersion = null;
            try {
                latestVersion = UpdatesHelper.GetLatestVersion();
            } catch (CouldNotCheckForUpdatesException) {
                MessageBox.Show("An error occured while checking for updates. Please retry later.", "Bandcamp Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (currentVersion.CompareTo(latestVersion) < 0) {
                // The latest version is newer than the current one
                String msgBoxText =
                    "A new version is available:" + Environment.NewLine +
                    "Current version = " + currentVersion + Environment.NewLine +
                    "Latest version = " + latestVersion + Environment.NewLine + Environment.NewLine +
                    "Would you like to go to the project website in order to download the latest version?";
                if (MessageBox.Show(msgBoxText, "Bandcamp Downloader", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes) == MessageBoxResult.Yes) {
                    Process.Start(Constants.ProjectWebsite);
                }
            } else {
                MessageBox.Show("You already have the latest version available (" + currentVersion + ").", "Bandcamp Downloader", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}