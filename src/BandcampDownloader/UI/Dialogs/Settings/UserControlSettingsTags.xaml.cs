using System.Windows.Controls;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsTags : IUserControlSettings
{
    public UserControlSettingsTags()
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
        TextBoxFileNameFormat.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        CheckBoxModifyTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        ComboBoxAlbumArtist.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        ComboBoxAlbumTitle.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        ComboBoxArtist.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        ComboBoxComments.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        ComboBoxLyrics.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        ComboBoxTrackNumber.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        ComboBoxTrackTitle.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        ComboBoxYear.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.ValidateWithoutUpdate();
    }
}
