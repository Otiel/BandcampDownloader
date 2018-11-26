using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsAdvanced: UserControl {

        public UserControlSettingsAdvanced() {
            InitializeComponent();
            // Save data context for bindings
            DataContext = App.userSettings;
        }

        /// <summary>
        /// Loads settings from WindowMain.userSettings.
        /// </summary>
        public void LoadSettings() {
            // Reload DataContext in case settings have changed
            DataContext = App.userSettings;
            // No need to call UpdateTarget, it is done automatically
        }

        /// <summary>
        /// Saves settings to WindowMain.userSettings.
        /// </summary>
        public void SaveSettings() {
            textBoxAllowedFileSizeDifference.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            textBoxDownloadMaxTries.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            textBoxDownloadRetryCooldown.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            textBoxDownloadRetryExponent.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox) sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
        }
    }
}