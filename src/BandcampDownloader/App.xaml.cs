using System;
using System.Net;
using System.Windows;
using Config.Net;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BandcampDownloader
{
    public partial class App : Application
    {
        /// <summary>
        /// Random class used to create random numbers.
        /// </summary>
        public static readonly Random Random = new Random();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The settings chosen by the user.
        /// </summary>
        public static IUserSettings UserSettings { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeLogger();

            // Manage unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            LogAppProperties();

            // Define the default security protocol to use for connection as TLS (#109)
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            InitializeSettings();
            LanguageHelper.ApplyLanguage(UserSettings.Language);
            ThemeHelper.ApplySkin(UserSettings.Theme);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogUnhandledExceptionToFile((Exception) e.ExceptionObject);

            MessageBox.Show(string.Format(BandcampDownloader.Properties.Resources.messageBoxUnhandledException, Constants.UrlIssues), "Bandcamp Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Initializes the logger component.
        /// </summary>
        private void InitializeLogger()
        {
            var fileTarget = new FileTarget()
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
        private void InitializeSettings()
        {
            App.UserSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
            if (string.IsNullOrEmpty(UserSettings.DownloadsPath))
            {
                // Its default value cannot be set in settings as it isn't determined by a constant function
                App.UserSettings.DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
            }
        }

        private void LogAppProperties()
        {
            _logger.Info($"BandcampDownloader version: {Constants.AppVersion}");
            _logger.Log(LogLevel.Info, $".NET Framework version: {SystemVersionHelper.GetDotNetFrameworkVersion()}");
        }

        /// <summary>
        /// Writes the specified Exception to the application log file, along with the .NET version.
        /// </summary>
        /// <param name="exception">The Exception to log.</param>
        private void LogUnhandledExceptionToFile(Exception exception)
        {
            LogHelper.LogExceptionAndInnerExceptionsToFile(exception);
        }
    }
}