using System;
using System.Windows;
using Config.Net;

namespace BandcampDownloader {

    public partial class App: Application {
        /// <summary>
        /// The settings chosen by the user.
        /// </summary>
        public static IUserSettings userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            InitializeSettings();
        }

        /// <summary>
        /// Initializes data context for bindings between settings values and settings controls.
        /// This must be called before initializing UI forms.
        /// </summary>
        private void InitializeSettings() {
            if (String.IsNullOrEmpty(userSettings.DownloadsPath)) {
                // Its default value cannot be set in settings as it isn't determined by a constant function
                App.userSettings.DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
            }
            App.userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
        }
    }
}