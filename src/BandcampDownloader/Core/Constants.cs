using System.IO;
using System.Reflection;

namespace BandcampDownloader {

    internal static class Constants {
        /// <summary>
        /// The version number of BandcampDownloader.
        /// </summary>
        public static readonly string AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        /// <summary>
        /// The absolute path to the log file.
        /// </summary>
        public static readonly string LogFilePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\BandcampDownloader.log";
        /// <summary>
        /// The log file maximum size in bytes.
        /// </summary>
        public static readonly long MaxLogSize = 1024 * 1024;
        /// <summary>
        /// The URL redirecting to the changelog file on GitHub.
        /// </summary>
        public static readonly string UrlChangelog = "https://raw.githubusercontent.com/Otiel/BandcampDownloader/master/CHANGELOG.md";
        /// <summary>
        /// The URL redirecting to the help page on translating the app on GitHub.
        /// </summary>
        public static readonly string UrlHelpTranslate = "https://github.com/Otiel/BandcampDownloader/blob/master/docs/help-translate.md";
        /// <summary>
        /// The URL redirecting to the issues page on GitHub.
        /// </summary>
        public static readonly string UrlIssues = "https://github.com/Otiel/BandcampDownloader/issues";
        /// <summary>
        /// The URL redirecting to the latest release on GitHub.
        /// </summary>
        public static readonly string UrlLatestRelease = "https://github.com/Otiel/BandcampDownloader/releases/latest";
        /// <summary>
        /// The URL redirecting to the releases page on GitHub.
        /// </summary>
        public static readonly string UrlReleases = "https://github.com/Otiel/BandcampDownloader/releases";
        /// <summary>
        /// The URL redirecting to the zip file. Must be formatted with a version.
        /// </summary>
        public static readonly string UrlReleaseZip = "https://github.com/Otiel/BandcampDownloader/releases/download/v{0}/BandcampDownloader.zip";
        /// <summary>
        /// The website URL of BandcampDownloader.
        /// </summary>
        public static readonly string UrlWebsite = "https://github.com/Otiel/BandcampDownloader";
        /// <summary>
        /// The absolute path to the settings file.
        /// </summary>
        public static readonly string UserSettingsFilePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\BandcampDownloader.ini";
    }
}