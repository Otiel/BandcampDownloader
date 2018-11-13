using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Config.Net;

namespace BandcampDownloader {

    public partial class SettingsWindow: Window {
        private Boolean activeDownloads;

        /// <summary>
        /// Creates a new instance of SettingsWindow.
        /// </summary>
        /// <param name="activeDownloads">True if there are active downloads; false otherwise.</param>
        public SettingsWindow(Boolean activeDownloads) {
            InitializeComponent();

            this.activeDownloads = activeDownloads;

            // Save data context for bindings
            DataContext = MainWindow.userSettings;

            UpdateControlsState();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ButtonResetSettings_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Reset settings to their default values?", "Bandcamp Downloader", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                ResetSettings();
            }
        }

        private void CheckBoxResizeCoverArt_CheckedChanged(object sender, RoutedEventArgs e) {
            if (checkBoxResizeCoverArt == null || textBoxCoverArtMaxSize == null) {
                return;
            }

            if (activeDownloads) {
                textBoxCoverArtMaxSize.IsEnabled = false;
            } else {
                textBoxCoverArtMaxSize.IsEnabled = checkBoxResizeCoverArt.IsChecked.Value;
            }
        }

        private void CheckBoxSaveCoverArt_CheckedChanged(object sender, RoutedEventArgs e) {
            if (checkBoxCoverArtInFolder == null || checkBoxCoverArtInTags == null || checkBoxConvertToJpg == null) {
                return;
            }

            if (activeDownloads) {
                checkBoxConvertToJpg.IsEnabled = false;
                checkBoxResizeCoverArt.IsEnabled = false;
                textBoxCoverArtMaxSize.IsEnabled = false;
            } else {
                checkBoxConvertToJpg.IsEnabled = checkBoxCoverArtInFolder.IsChecked.Value || checkBoxCoverArtInTags.IsChecked.Value;
                checkBoxResizeCoverArt.IsEnabled = checkBoxCoverArtInFolder.IsChecked.Value || checkBoxCoverArtInTags.IsChecked.Value;
                textBoxCoverArtMaxSize.IsEnabled = checkBoxCoverArtInFolder.IsChecked.Value || checkBoxCoverArtInTags.IsChecked.Value;
            }
        }

        /// <summary>
        /// Resets settings to their default values.
        /// </summary>
        private void ResetSettings() {
            // Save the downloads path as we don't want to reset this setting
            String downloadsPath = MainWindow.userSettings.DownloadsLocation;

            File.Delete(Constants.UserSettingsFilePath);
            MainWindow.userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
            MainWindow.userSettings.DownloadsLocation = downloadsPath;
            DataContext = MainWindow.userSettings;
        }

        /// <summary>
        /// Updates controls state.
        /// </summary>
        private void UpdateControlsState() {
            // Update controls state based on the settings values (possibly stored in the settings file)
            textBoxCoverArtMaxSize.IsEnabled = MainWindow.userSettings.ResizeCoverArt;
            checkBoxConvertToJpg.IsEnabled = MainWindow.userSettings.SaveCoverArtInFolder || MainWindow.userSettings.SaveCoverArtInTags;
            checkBoxResizeCoverArt.IsEnabled = MainWindow.userSettings.SaveCoverArtInFolder || MainWindow.userSettings.SaveCoverArtInTags;
            textBoxCoverArtMaxSize.IsEnabled = MainWindow.userSettings.SaveCoverArtInFolder || MainWindow.userSettings.SaveCoverArtInTags;

            // Then update controls state based on active downloads
            buttonResetSettings.IsEnabled = !activeDownloads;
            checkBoxConvertToJpg.IsEnabled = !activeDownloads;
            checkBoxCoverArtInFolder.IsEnabled = !activeDownloads;
            checkBoxCoverArtInTags.IsEnabled = !activeDownloads;
            checkBoxDownloadDiscography.IsEnabled = !activeDownloads;
            checkBoxOneAlbumAtATime.IsEnabled = !activeDownloads;
            checkBoxResizeCoverArt.IsEnabled = !activeDownloads;
            checkBoxRetrieveFilesizes.IsEnabled = !activeDownloads;
            checkBoxTag.IsEnabled = !activeDownloads;
            textBoxCoverArtMaxSize.IsEnabled = !activeDownloads;
            textBoxFilenameFormat.IsEnabled = !activeDownloads;
            imageInfo.Source = activeDownloads ? new BitmapImage(new Uri("/Resources/ExclamationSmall.png", UriKind.Relative)) : new BitmapImage(new Uri("/Resources/InformationSmallWhite.png", UriKind.Relative));
            labelInfo.Content = activeDownloads ? "Stop downloads in order to change your settings." : "Settings are automatically saved after change.";
        }
    }
}