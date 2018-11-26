using System.Windows.Controls;

namespace BandcampDownloader {

    public partial class UserControlSettingsTags: UserControl {

        public UserControlSettingsTags() {
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
            textBoxFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            checkBoxModifyTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            comboBoxAlbum.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxAlbumArtist.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxArtist.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxComments.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxLyrics.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxTitle.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxTrackNumber.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            comboBoxYear.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
        }
    }
}