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
            checkBoxDownloadDiscography.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxOneAlbumAtATime.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxRetrieveFilesizes.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxTag.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            textBoxFilenameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}