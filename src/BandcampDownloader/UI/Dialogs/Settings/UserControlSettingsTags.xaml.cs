﻿using System.Windows.Controls;

namespace BandcampDownloader.UI.Dialogs.Settings;

public sealed partial class UserControlSettingsTags : UserControl, IUserControlSettings
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
        textBoxFileNameFormat.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        checkBoxModifyTags.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        comboBoxAlbumArtist.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        comboBoxAlbumTitle.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        comboBoxArtist.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        comboBoxComments.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        comboBoxLyrics.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        comboBoxTrackNumber.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        comboBoxTrackTitle.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        comboBoxYear.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
    }
}
