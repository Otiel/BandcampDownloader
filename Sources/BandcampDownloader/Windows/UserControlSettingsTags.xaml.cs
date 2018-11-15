using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsTags: UserControl {

        public UserControlSettingsTags() {
            InitializeComponent();
            // Save data context for bindings
            DataContext = MainWindow.userSettings;
        }

        /// <summary>
        /// Save all settings.
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