using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsAdvanced: UserControl {

        public UserControlSettingsAdvanced() {
            InitializeComponent();
            // Save data context for bindings
            DataContext = WindowMain.userSettings;
        }

        /// <summary>
        /// Save all settings.
        /// </summary>
        public void SaveSettings() {
            textBoxAllowableFileSizeDifference.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            textBoxDownloadMaxTries.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            textBoxDownloadRetryCooldown.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            textBoxDownloadRetryExponential.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox) sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
        }
    }
}