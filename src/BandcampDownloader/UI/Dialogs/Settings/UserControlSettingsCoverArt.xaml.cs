using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BandcampDownloader.Core.DependencyInjection;
using BandcampDownloader.Settings;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsCoverArt : IUserControlSettings
{
    public UserControlSettingsCoverArt()
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
        CheckBoxSaveCoverArtInFolder.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        TextBoxCoverArtFileNameFormat.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        CheckBoxCoverArtInFolderConvertToJpg.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        CheckBoxCoverArtInFolderResize.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        TextBoxCoverArtInFolderMaxSize.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        CheckBoxSaveCoverArtInTags.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        CheckBoxCoverArtInTagsConvertToJpg.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        CheckBoxCoverArtInTagsResize.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        TextBoxCoverArtInTagsMaxSize.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.ValidateWithoutUpdate();
    }
}
