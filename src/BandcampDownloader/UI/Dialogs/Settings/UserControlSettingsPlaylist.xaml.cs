using System.Windows.Controls;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsPlaylist : IUserControlSettings
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
        CheckBoxCreatePlaylist.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        CheckBoxM3UExtended.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        ComboBoxPlaylistFormat.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
        TextBoxPlaylistFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }
}
