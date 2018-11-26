using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsGeneral: UserControl {

        public UserControlSettingsGeneral() {
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
            checkBoxCheckForUpdates.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxVerboseLog.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        }
    }
}