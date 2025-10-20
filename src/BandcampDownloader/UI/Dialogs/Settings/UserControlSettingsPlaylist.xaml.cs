using System.Windows.Controls;

namespace BandcampDownloader;

public partial class UserControlSettingsPlaylist : UserControl, IUserControlSettings
{
    public UserControlSettingsPlaylist()
    {
        InitializeComponent();
        // Save data context for bindings
        DataContext = App.UserSettings;
    }

    /// <summary>
    /// Cancels the changes already applied.
    /// </summary>
    public void CancelChanges()
    {
        // Nothing to "unapply"
    }

    /// <summary>
    /// Loads settings from App.UserSettings.
    /// </summary>
    public void LoadSettings()
    {
        // Reload DataContext in case settings have changed
        DataContext = App.UserSettings;
        // No need to call UpdateTarget, it is done automatically
    }

    /// <summary>
    /// Saves settings to App.UserSettings.
    /// </summary>
    public void SaveSettings()
    {
        checkBoxCreatePlaylist.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        checkBoxM3uExtended.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        comboBoxPlaylistFormat.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
        textBoxPlaylistFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }
}
