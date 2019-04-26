using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsNetwork: UserControl {

        public UserControlSettingsNetwork() {
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
            radioButtonManualProxy.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();
            radioButtonNoProxy.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();
            radioButtonSystemProxy.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();
            textBoxHttpAddress.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            textBoxHttpPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox) sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
        }
    }
}