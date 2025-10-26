using System.Windows.Controls.Primitives;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Settings;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsDownloads : IUserControlSettings
{
    public UserControlSettingsDownloads()
    {
        var userSettings = DependencyInjectionHelper.GetService<ISettingsService>().GetUserSettings();

        InitializeComponent();
        // Save data context for bindings
        DataContext = userSettings;
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
    public void LoadSettings(IUserSettings userSettings)
    {
        // Reload DataContext in case settings have changed
        DataContext = userSettings;
        // No need to call UpdateTarget, it is done automatically
    }

    /// <summary>
    /// Saves settings to _userSettings.
    /// </summary>
    public void SaveSettings()
    {
        CheckBoxOneAlbumAtATime.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        CheckBoxRetrieveFilesSize.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
    }
}
