using System;
using System.Net;
using System.Windows;
using BandcampDownloader.Core;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Helpers;
using BandcampDownloader.UI.Dialogs;
using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;

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
        InitializeLogger();
        var container = DependencyInjectionHelper.InitializeContainer();

        // Manage unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        LogAppProperties();

        // Define the default security protocol to use for connection as TLS (#109)
#pragma warning disable SYSLIB0014
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#pragma warning restore SYSLIB0014

        InitializeSettings();
        LanguageHelper.ApplyLanguage(UserSettings.Language);
        ThemeHelper.ApplySkin(UserSettings.Theme);

        var windowMain = container.GetRequiredService<WindowMain>();
        windowMain.Show();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogUnhandledExceptionToFile((Exception)e.ExceptionObject);

        MessageBox.Show(string.Format(BandcampDownloader.Properties.Resources.messageBoxUnhandledException, Constants.UrlIssues), "Bandcamp Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Initializes the logger component.
    /// </summary>
    private static void InitializeLogger()
    {
        var fileTarget = new FileTarget
        {
            FileName = Constants.LogFilePath,
            Layout = "${longdate}  ${level:uppercase=true:padding=-5:padCharacter= }  ${message}",
            ArchiveAboveSize = Constants.MaxLogSize,
            MaxArchiveFiles = 1,
        };

        var config = new LoggingConfiguration();
        config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);

        LogManager.Configuration = config;
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

    /// <summary>
    /// Writes the specified Exception to the application log file, along with the .NET version.
    /// </summary>
    /// <param name="exception">The Exception to log.</param>
    private static void LogUnhandledExceptionToFile(Exception exception)
    {
        LogHelper.LogExceptionAndInnerExceptionsToFile(exception);
    }
}
