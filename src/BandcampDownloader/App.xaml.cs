using System.Runtime.InteropServices;
using System.Windows;
using BandcampDownloader.Core;
using BandcampDownloader.Core.DependencyInjection;
using BandcampDownloader.Core.Localization;
using BandcampDownloader.Core.Logging;
using BandcampDownloader.Core.Themes;
using BandcampDownloader.Settings;
using BandcampDownloader.UI.Dialogs;
using NLog;

namespace BandcampDownloader;

internal sealed partial class App
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    protected override void OnStartup(StartupEventArgs e)
    {
        var container = DependencyInjectionHelper.InitializeContainer();

        // 1. Initialize the logger first in order to be able to log any early errors
        var loggingService = container.GetService<ILoggingService>();
        loggingService.InitializeLogger();

        // 2. Register the handler for unhandled exception in order to catch and log early unhandled exceptions
        var exceptionHandler = container.GetService<IExceptionHandler>();
        exceptionHandler.RegisterUnhandledExceptionHandler();

        // 3. Log the application properties
        LogAppProperties();

        // 4. Initialize less critical services
        InitializeCoreServices(container);

        // 5. Log the user settings
        LogUserSettings(container);

        // 6. Open the main window
        var windowMain = container.GetService<WindowMain>();
        windowMain.Show();
    }

    private static void InitializeCoreServices(IContainer container)
    {
        var settingsService = container.GetService<ISettingsService>();
        var userSettings = settingsService.InitializeSettings();

        var languageService = container.GetService<ILanguageService>();
        languageService.ApplyLanguage(userSettings.Language);

        var themeService = container.GetService<IThemeService>();
        themeService.ApplySkin(userSettings.Theme);
    }

    private static void LogAppProperties()
    {
        _logger.Info("┳┓     ┓          ┳┓       ┓     ┓");
        _logger.Info("┣┫┏┓┏┓┏┫┏┏┓┏┳┓┏┓  ┃┃┏┓┓┏┏┏┓┃┏┓┏┓┏┫┏┓┏┓");
        _logger.Info("┻┛┗┻┛┗┗┻┗┗┻┛┗┗┣┛  ┻┛┗┛┗┻┛┛┗┗┗┛┗┻┗┻┗ ┛");
        _logger.Info("              ┛");
        _logger.Info($"BandcampDownloader version: {Constants.APP_VERSION_FORMATTED}");
        _logger.Info($"Framework description: {RuntimeInformation.FrameworkDescription}");
        _logger.Info($"OS architecture: {RuntimeInformation.OSArchitecture}");
        _logger.Info($"OS description: {RuntimeInformation.OSDescription}");
        _logger.Info($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
        _logger.Info($"Runtime identifier: {RuntimeInformation.RuntimeIdentifier}");
    }

    private static void LogUserSettings(IContainer container)
    {
        var settingsService = container.GetService<ISettingsService>();
        var userSettingsJson = settingsService.GetUserSettingsInJson();

        _logger.Info($"Settings: {userSettingsJson}");
    }
}
