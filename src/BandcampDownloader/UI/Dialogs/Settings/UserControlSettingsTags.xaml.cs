using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Settings;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsTags : IUserControlSettings
{
    private readonly IUserSettings _userSettings;

    public UserControlSettingsTags()
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
        TextBoxFileNameFormat.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        CheckBoxModifyTags.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        ComboBoxAlbumArtist.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        ComboBoxAlbumTitle.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        ComboBoxArtist.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        ComboBoxComments.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        ComboBoxLyrics.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        ComboBoxTrackNumber.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        ComboBoxTrackTitle.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
        ComboBoxYear.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.ValidateWithoutUpdate();
    }
}
