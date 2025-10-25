using System.Reflection;

namespace BandcampDownloader.Core;

internal static class Constants
{
    /// <summary>
    /// The version number of BandcampDownloader.
    /// </summary>
    public static readonly string AppVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3);

    /// <summary>
    /// The URL redirecting to the changelog file on GitHub.
    /// </summary>
    public const string UrlChangelog = "https://raw.githubusercontent.com/Otiel/BandcampDownloader/master/CHANGELOG.md";

    /// <summary>
    /// The URL redirecting to the help page on translating the app on GitHub.
    /// </summary>
    public const string UrlHelpTranslate = "https://github.com/Otiel/BandcampDownloader/blob/master/docs/help-translate.md";

    /// <summary>
    /// The URL redirecting to the issues page on GitHub.
    /// </summary>
    public const string UrlIssues = "https://github.com/Otiel/BandcampDownloader/issues";

    /// <summary>
    /// The URL redirecting to the latest release on GitHub.
    /// </summary>
    public const string UrlLatestRelease = "https://github.com/Otiel/BandcampDownloader/releases/latest";

    /// <summary>
    /// The URL redirecting to the releases page on GitHub.
    /// </summary>
    public const string UrlReleases = "https://github.com/Otiel/BandcampDownloader/releases";

    /// <summary>
    /// The URL redirecting to the zip file. Must be formatted with a version.
    /// </summary>
    public const string UrlReleaseZip = "https://github.com/Otiel/BandcampDownloader/releases/download/v{0}/BandcampDownloader.zip";

    /// <summary>
    /// The website URL of BandcampDownloader.
    /// </summary>
    public const string UrlWebsite = "https://github.com/Otiel/BandcampDownloader";
}
