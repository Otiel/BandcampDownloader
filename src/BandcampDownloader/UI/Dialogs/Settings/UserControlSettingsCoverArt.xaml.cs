using System.Windows.Controls;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsCoverArt : IUserControlSettings
{
    public UserControlSettingsCoverArt()
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
        CheckBoxSaveCoverArtInFolder.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        TextBoxCoverArtFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        CheckBoxCoverArtInFolderConvertToJpg.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        CheckBoxCoverArtInFolderResize.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        TextBoxCoverArtInFolderMaxSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        CheckBoxSaveCoverArtInTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        CheckBoxCoverArtInTagsConvertToJpg.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        CheckBoxCoverArtInTagsResize.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        TextBoxCoverArtInTagsMaxSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.ValidateWithoutUpdate();
    }
}
