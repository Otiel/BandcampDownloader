using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsTags: UserControl {

        public UserControlSettingsTags() {
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
            textBoxFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            checkBoxModifyTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            comboBoxAlbumArtist.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxAlbumTitle.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxArtist.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxComments.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxLyrics.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxTrackNumber.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxTrackTitle.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxYear.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
        }
    }
}