using System.Windows.Controls;
using BandcampDownloader.Core.DependencyInjection;
using BandcampDownloader.Settings;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsAdvanced : IUserControlSettings
{
    public UserControlSettingsAdvanced()
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
        TextBoxAllowedFileSizeDifference.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        TextBoxDownloadMaxTries.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        TextBoxDownloadRetryCooldown.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        TextBoxDownloadRetryExponent.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.ValidateWithoutUpdate();
    }
}
