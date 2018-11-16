using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsTags: UserControl {

        public UserControlSettingsTags() {
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
            textBoxFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            checkBoxTagTracks.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagRemoveComments.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagSaveAlbum.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagSaveAlbumArtist.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagSaveArtist.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagSaveLyrics.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagSaveTitle.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagSaveTrackNumber.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTagSaveYear.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        }
    }
}