using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using BandcampDownloader.Core;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Helpers;
using BandcampDownloader.Themes;
using BandcampDownloader.UI.Dialogs.Update;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsGeneral : IUserControlSettings
{
    private readonly IThemeService _themeService;

    public UserControlSettingsGeneral()
    {
        _themeService = DependencyInjectionHelper.GetService<IThemeService>();

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
        if ((Language)ComboBoxLanguage.SelectedValue != App.UserSettings.Language)
        {
            LanguageHelper.ApplyLanguage(App.UserSettings.Language);
        }

        // Revert the theme only if it has been changed
        if ((Skin)ComboBoxTheme.SelectedItem != App.UserSettings.Theme)
        {
            _themeService.ApplySkin(App.UserSettings.Theme);
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
        CheckBoxCheckForUpdates.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        CheckBoxEnableApplicationSounds.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        CheckBoxVerboseLog.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
        ComboBoxLanguage.GetBindingExpression(Selector.SelectedValueProperty)?.UpdateSource();
        ComboBoxTheme.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
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
        if (currentVersion!.CompareTo(latestVersion) < 0)
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
        if (!ComboBoxLanguage.IsLoaded)
        {
            return;
        }

        // Apply selected language
        LanguageHelper.ApplyLanguage((Language)ComboBoxLanguage.SelectedValue);
    }

    private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ComboBoxTheme.IsLoaded)
        {
            return;
        }

        // Apply selected theme
        _themeService.ApplySkin((Skin)ComboBoxTheme.SelectedItem);
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        UrlHelper.OpenUrlInBrowser(e.Uri.AbsoluteUri);
        e.Handled = true;
    }
}
