using System;
using System.Net;
using System.Windows;
using BandcampDownloader.Core;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Helpers;
using BandcampDownloader.Localization;
using BandcampDownloader.Logging;
using BandcampDownloader.Settings;
using BandcampDownloader.Themes;
using BandcampDownloader.UI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace BandcampDownloader;

internal sealed partial class App
{
    /// <summary>
    /// Random class used to create random numbers.
    /// </summary>
    public static readonly Random Random = new();

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    protected override void OnStartup(StartupEventArgs e)
    {
        var container = DependencyInjectionHelper.InitializeContainer();

        var loggingService = container.GetRequiredService<ILoggingService>();
        loggingService.InitializeLogger();

        var exceptionHandler = container.GetRequiredService<IExceptionHandler>();
        exceptionHandler.RegisterUnhandledExceptionHandler();

        LogAppProperties();

        // Define the default security protocol to use for connection as TLS (#109)
#pragma warning disable SYSLIB0014
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#pragma warning restore SYSLIB0014

        var settingsService = container.GetRequiredService<ISettingsService>();
        var userSettings = settingsService.InitializeSettings();

        var languageService = container.GetRequiredService<ILanguageService>();
        languageService.ApplyLanguage(userSettings.Language);

        var themeService = container.GetRequiredService<IThemeService>();
        themeService.ApplySkin(userSettings.Theme);

        var windowMain = container.GetRequiredService<WindowMain>();
        windowMain.Show();
    }

    private static void LogAppProperties()
    {
        _logger.Info($"BandcampDownloader version: {Constants.AppVersion}");
        _logger.Info($".NET Framework version: {SystemVersionHelper.GetDotNetFrameworkVersion()}");
    }
}
