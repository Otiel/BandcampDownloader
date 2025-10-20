using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsNetwork : IUserControlSettings
{
    public UserControlSettingsNetwork()
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
        RadioButtonManualProxy.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        RadioButtonNoProxy.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        RadioButtonSystemProxy.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        TextBoxHttpAddress.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        TextBoxHttpPort.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        CheckBoxUseHttpInsteadOfHttps.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.ValidateWithoutUpdate();
    }
}
