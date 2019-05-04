using System;
using System.IO;
using System.Reflection;

namespace BandcampDownloader {

    internal static class Constants {
        /// <summary>
        /// The version number of BandcampDownloader.
        /// </summary>
        public static readonly String AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        /// <summary>
        /// The URL redirecting to the changelog file on GitHub.
        /// </summary>
        public static readonly String ChangelogUrl = "https://raw.githubusercontent.com/Otiel/BandcampDownloader/master/CHANGELOG.md";
        /// <summary>
        /// The URL redirecting to the help page on translating the app on GitHub.
        /// </summary>
        public static readonly String HelpTranslateWebsite = "https://github.com/Otiel/BandcampDownloader/blob/master/docs/help-translate.md";
        /// <summary>
        /// The URL redirecting to the latest release on GitHub.
        /// </summary>
        public static readonly String LatestReleaseWebsite = "https://github.com/Otiel/BandcampDownloader/releases/latest";
        /// <summary>
        /// The absolute path to the log file.
        /// </summary>
        public static readonly String LogFilePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\BandcampDownloader.log";
        /// <summary>
        /// The log file maximum size in bytes.
        /// </summary>
        public static readonly long MaxLogSize = 1024 * 1024;
        /// <summary>
        /// The website URL of BandcampDownloader.
        /// </summary>
        public static readonly String ProjectWebsite = "https://github.com/Otiel/BandcampDownloader";
        /// <summary>
        /// The absolute path to the settings file.
        /// </summary>
        public static readonly String UserSettingsFilePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\BandcampDownloader.ini";
        /// <summary>
        /// The URL redirecting to the zip file. Must be formatted with a version.
        /// </summary>
        public static readonly String ZipUrl = "https://github.com/Otiel/BandcampDownloader/releases/download/v{0}/BandcampDownloader.zip";
    }
}