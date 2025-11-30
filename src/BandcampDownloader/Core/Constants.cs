using System;
using System.Reflection;

namespace BandcampDownloader.Core;

internal static class Constants
{
    /// <summary>
    /// The version of BandcampDownloader.
    /// </summary>
    public static readonly Version AppVersion = Assembly.GetEntryAssembly()?.GetName().Version;

    /// <summary>
    /// The formatted version (X.Y.Z) of BandcampDownloader.
    /// </summary>
    public static readonly string AppVersionFormatted = AppVersion.ToString(3);

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
    /// The website URL of BandcampDownloader.
    /// </summary>
    public const string UrlWebsite = "https://github.com/Otiel/BandcampDownloader";
}
