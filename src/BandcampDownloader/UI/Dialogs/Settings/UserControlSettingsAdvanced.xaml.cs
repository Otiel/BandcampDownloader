using System.Windows.Controls;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsAdvanced : IUserControlSettings
{
    public UserControlSettingsAdvanced()
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
