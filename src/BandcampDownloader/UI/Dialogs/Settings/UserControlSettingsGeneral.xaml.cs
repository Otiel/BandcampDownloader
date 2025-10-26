using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Helpers;
using BandcampDownloader.Localization;
using BandcampDownloader.Settings;
using BandcampDownloader.Themes;
using BandcampDownloader.UI.Dialogs.Update;
using BandcampDownloader.Updates;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsGeneral : IUserControlSettings
{
    private readonly ILanguageService _languageService;
    private readonly ISettingsService _userSettingsService;
    private readonly IThemeService _themeService;
    private readonly IUpdatesService _updatesService;

    public UserControlSettingsGeneral()
    {
        _languageService = DependencyInjectionHelper.GetService<ILanguageService>();
        _userSettingsService = DependencyInjectionHelper.GetService<ISettingsService>();
        _themeService = DependencyInjectionHelper.GetService<IThemeService>();
        _updatesService = DependencyInjectionHelper.GetService<IUpdatesService>();
        var userSettings = _userSettingsService.GetUserSettings();

        InitializeComponent();
        // Save data context for bindings
        DataContext = userSettings;
    }

    /// <summary>
    /// Cancels the changes already applied.
    /// </summary>
    public void CancelChanges()
    {
        var userSettings = _userSettingsService.GetUserSettings();

        // Revert the language only if it has been changed
        if ((Language)ComboBoxLanguage.SelectedValue != userSettings.Language)
        {
            _languageService.ApplyLanguage(userSettings.Language);
        }

        // Revert the theme only if it has been changed
        if ((Skin)ComboBoxTheme.SelectedItem != userSettings.Theme)
        {
            _themeService.ApplySkin(userSettings.Theme);
        }
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
            latestVersion = await _updatesService.GetLatestVersionAsync();
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
        _languageService.ApplyLanguage((Language)ComboBoxLanguage.SelectedValue);
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
