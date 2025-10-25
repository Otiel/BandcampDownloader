using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Settings;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsPlaylist : IUserControlSettings
{
    private readonly IUserSettings _userSettings;

    public UserControlSettingsPlaylist()
    {
        _userSettings = DependencyInjectionHelper.GetService<ISettingsService>().GetUserSettings();

        InitializeComponent();
        // Save data context for bindings
        DataContext = _userSettings;
    }

    /// <summary>
    /// Cancels the changes already applied.
    /// </summary>
    public void CancelChanges()
    {
        // Nothing to "unapply"
    }

    /// <summary>
    /// Loads settings from _userSettings.
    /// </summary>
    public void LoadSettings()
    {
        // Reload DataContext in case settings have changed
        DataContext = _userSettings;
        // No need to call UpdateTarget, it is done automatically
    }

    /// <summary>
    /// Saves settings to _userSettings.
    /// </summary>
    public void SaveSettings()
    {
        CheckBoxCreatePlaylist.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        CheckBoxM3UExtended.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        ComboBoxPlaylistFormat.GetBindingExpression(Selector.SelectedValueProperty)?.UpdateSource();
        TextBoxPlaylistFileNameFormat.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}
