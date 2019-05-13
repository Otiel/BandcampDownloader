using System;
using System.Windows;
using Config.Net;

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
            InitializeSettings();
            LanguageHelper.ApplyLanguage(UserSettings.Language);
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
    }
}