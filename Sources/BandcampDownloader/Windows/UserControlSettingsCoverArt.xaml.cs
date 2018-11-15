using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsCoverArt: UserControl {

        public UserControlSettingsCoverArt() {
            InitializeComponent();
            // Save data context for bindings
            DataContext = WindowMain.userSettings;
        }

        /// <summary>
        /// Loads settings from WindowMain.userSettings.
        /// </summary>
        public void LoadSettings() {
            // Reload DataContext in case settings have changed
            DataContext = WindowMain.userSettings;
            // No need to call UpdateTarget, it is done automatically
        }

        /// <summary>
        /// Saves settings to WindowMain.userSettings.
        /// </summary>
        public void SaveSettings() {
            checkBoxConvertToJpg.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxCoverArtInFolder.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxCoverArtInTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxResizeCoverArt.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            textBoxCoverArtMaxSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox) sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
        }
    }
}