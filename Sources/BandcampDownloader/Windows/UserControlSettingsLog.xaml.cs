using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsLog: UserControl {

        public UserControlSettingsLog() {
            InitializeComponent();
            // Save data context for bindings
            DataContext = MainWindow.userSettings;
        }

        /// <summary>
        /// Save all settings.
        /// </summary>
        public void SaveSettings() {
            checkBoxAutoScrollLog.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxVerboseLog.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        }
    }
}