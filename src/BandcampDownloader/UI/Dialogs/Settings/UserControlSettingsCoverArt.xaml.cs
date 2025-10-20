using System.Windows.Controls;

namespace BandcampDownloader.UI.Dialogs.Settings;

public partial class UserControlSettingsCoverArt : UserControl, IUserControlSettings
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
        checkBoxSaveCoverArtInFolder.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        textBoxCoverArtFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        checkBoxCoverArtInFolderConvertToJpg.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        checkBoxCoverArtInFolderResize.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        textBoxCoverArtInFolderMaxSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        checkBoxSaveCoverArtInTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        checkBoxCoverArtInTagsConvertToJpg.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        checkBoxCoverArtInTagsResize.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        textBoxCoverArtInTagsMaxSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
    }
}
