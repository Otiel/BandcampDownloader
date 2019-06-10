using System;
using System.Windows;
using Config.Net;
using NLog;
using NLog.Config;
using NLog.Targets;
using WpfMessageBoxLibrary;

namespace BandcampDownloader {

    public partial class App: Application {
        /// <summary>
        /// Random class used to create random numbers.
        /// </summary>
        public static readonly Random Random = new Random();

        /// <summary>
        /// The settings chosen by the user.
        /// </summary>
        public static IUserSettings UserSettings { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            InitializeLogger();

            // Manage unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            InitializeSettings();
            LanguageHelper.ApplyLanguage(UserSettings.Language);
            ThemeHelper.ApplySkin(UserSettings.Theme);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            LogExceptionToFile((Exception) e.ExceptionObject);


            var msgProperties = new WpfMessageBoxProperties() {
                Button = MessageBoxButton.OK,
                ButtonOkText = BandcampDownloader.Properties.Resources.messageBoxButtonOK,
                Image = MessageBoxImage.Error,
                Text = String.Format(BandcampDownloader.Properties.Resources.messageBoxUnhandledException, Constants.UrlIssues),
                Title = "Bandcamp Downloader",
            };
            WpfMessageBox.Show(ref msgProperties);
        }

        /// <summary>
        /// Initializes the logger component.
        /// </summary>
        private void InitializeLogger() {
            var fileTarget = new FileTarget() {
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
        /// Initializes data context for bindings between settings values and settings controls.
        /// This must be called before initializing UI forms.
        /// </summary>
        private void InitializeSettings() {
            App.UserSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
            if (string.IsNullOrEmpty(UserSettings.DownloadsPath)) {
                // Its default value cannot be set in settings as it isn't determined by a constant function
                App.UserSettings.DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
            }
        }

        /// <summary>
        /// Writes the specified Exception to the application log file.
        /// </summary>
        /// <param name="exception">The Exception to log.</param>
        private void LogExceptionToFile(Exception exception) {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Log(LogLevel.Fatal, String.Format("{0} {1}", exception.GetType().ToString(), exception.Message));
            logger.Log(LogLevel.Fatal, exception.StackTrace);
        }
    }
}