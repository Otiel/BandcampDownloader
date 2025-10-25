using System;
using System.Net;
using System.Windows;
using BandcampDownloader.Core;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Helpers;
using BandcampDownloader.Logging;
using BandcampDownloader.Themes;
using BandcampDownloader.UI.Dialogs;
using Config.Net;
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

    /// <summary>
    /// The settings chosen by the user.
    /// </summary>
    public static IUserSettings UserSettings { get; set; }

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

        InitializeSettings();
        LanguageHelper.ApplyLanguage(UserSettings.Language);

        var themeService = container.GetRequiredService<IThemeService>();
        themeService.ApplySkin(UserSettings.Theme);

        var windowMain = container.GetRequiredService<WindowMain>();
        windowMain.Show();
    }

    /// <summary>
    /// Initializes data context for bindings between settings values and settings controls. This must be called
    /// before initializing UI forms.
    /// </summary>
    private static void InitializeSettings()
    {
        UserSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
        if (string.IsNullOrEmpty(UserSettings.DownloadsPath))
        {
            // Its default value cannot be set in settings as it isn't determined by a constant function
            UserSettings.DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
        }
    }

    private static void LogAppProperties()
    {
        _logger.Info($"BandcampDownloader version: {Constants.AppVersion}");
        _logger.Info($".NET Framework version: {SystemVersionHelper.GetDotNetFrameworkVersion()}");
    }
}
