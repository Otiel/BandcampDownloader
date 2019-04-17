using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsCoverArt: UserControl {

        public UserControlSettingsCoverArt() {
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
            checkBoxSaveCoverArtInFolder.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            textBoxCoverArtFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            checkBoxCoverArtInFolderConvertToJpg.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxCoverArtInFolderResize.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            textBoxCoverArtInFolderMaxSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            checkBoxSaveCoverArtInTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxCoverArtInTagsConvertToJpg.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxCoverArtInTagsResize.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            textBoxCoverArtInTagsMaxSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox) sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
        }
    }
}