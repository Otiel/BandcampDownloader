﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using BandcampDownloader.Core;
using BandcampDownloader.Helpers;
using BandcampDownloader.UI.Dialogs.Update;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs.Settings;

public sealed partial class UserControlSettingsGeneral : IUserControlSettings
{
    public UserControlSettingsGeneral()
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
        // Revert the language only if it has been changed
        if ((Language)comboBoxLanguage.SelectedValue != App.UserSettings.Language)
        {
            LanguageHelper.ApplyLanguage(App.UserSettings.Language);
        }

        // Revert the theme only if it has been changed
        if ((Skin)comboBoxTheme.SelectedItem != App.UserSettings.Theme)
        {
            ThemeHelper.ApplySkin(App.UserSettings.Theme);
        }
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
        checkBoxCheckForUpdates.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        checkBoxEnableApplicationSounds.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        checkBoxVerboseLog.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        comboBoxLanguage.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
        comboBoxTheme.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
    }

    private async void ButtonCheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        Version latestVersion;
        try
        {
            latestVersion = await UpdatesHelper.GetLatestVersionAsync();
        }
        catch (CouldNotCheckForUpdatesException)
        {
            var msgProperties = new WpfMessageBoxProperties
            {
                Button = MessageBoxButton.OK,
                ButtonOkText = Properties.Resources.messageBoxButtonOK,
                Image = MessageBoxImage.Error,
                Text = Properties.Resources.messageBoxCheckForUpdatesError,
                Title = "Bandcamp Downloader",
            };
            WpfMessageBox.Show(Window.GetWindow(this), ref msgProperties);

            return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (currentVersion.CompareTo(latestVersion) < 0)
        {
            // The latest version is newer than the current one
            var windowUpdate = new WindowUpdate
            {
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            windowUpdate.Show();
        }
        else
        {
            var msgProperties = new WpfMessageBoxProperties
            {
                Button = MessageBoxButton.OK,
                ButtonOkText = Properties.Resources.messageBoxButtonOK,
                Image = MessageBoxImage.Information,
                Text = string.Format(Properties.Resources.messageBoxNoUpdateAvailable, currentVersion.ToString(3)),
                Title = "Bandcamp Downloader",
            };
            WpfMessageBox.Show(Window.GetWindow(this), ref msgProperties);
        }
    }

    private void ComboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!comboBoxLanguage.IsLoaded)
        {
            return;
        }

        // Apply selected language
        LanguageHelper.ApplyLanguage((Language)comboBoxLanguage.SelectedValue);
    }

    private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!comboBoxTheme.IsLoaded)
        {
            return;
        }

        // Apply selected theme
        ThemeHelper.ApplySkin((Skin)comboBoxTheme.SelectedItem);
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        e.Handled = true;
    }
}
