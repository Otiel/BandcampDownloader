using System;
using System.Globalization;
using System.Windows;
using Config.Net;
using WPFLocalizeExtension.Engine;

namespace BandcampDownloader {

    public partial class App: Application {

        /// <summary>
        /// The settings chosen by the user.
        /// </summary>
        public static IUserSettings UserSettings { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            InitializeSettings();
            LoadLanguage();
        }

        /// <summary>
        /// Initializes data context for bindings between settings values and settings controls.
        /// This must be called before initializing UI forms.
        /// </summary>
        private void InitializeSettings() {
            App.UserSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();
            if (String.IsNullOrEmpty(UserSettings.DownloadsPath)) {
                // Its default value cannot be set in settings as it isn't determined by a constant function
                App.UserSettings.DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
            }
        }

        /// <summary>
        /// Sets the CultureInfo according to the language saved in settings.
        /// </summary>
        private void LoadLanguage() {
            LocalizeDictionary.Instance.Culture = new CultureInfo(UserSettings.Language.ToString());
        }
    }
}