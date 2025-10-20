using System;
using System.Net;
using System.Threading.Tasks;
using BandcampDownloader.Core;

namespace BandcampDownloader.Helpers;

internal static class UpdatesHelper
{
    /// <summary>
    /// Returns the latest version available.
    /// </summary>
    public static async Task<Version> GetLatestVersionAsync()
    {
        // Note: GitHub uses a HTTP redirect to redirect from the generic latest release page to the actual latest release page

        // Retrieve the redirect page from the GitHub latest release page
        var request = HttpWebRequest.CreateHttp(Constants.UrlLatestRelease);
        request.AllowAutoRedirect = false;
        var redirectPage = "";
        try
        {
            using (var response = await request.GetResponseAsync())
            {
                redirectPage = ((HttpWebResponse)response).GetResponseHeader("Location");
                // redirectPage should be like "https://github.com/Otiel/BandcampDownloader/releases/tag/vX.X.X"
            }
        }
        catch
        {
            throw new CouldNotCheckForUpdatesException();
        }

        // Extract the version number from the URL
        string latestVersionNumber;
        try
        {
            latestVersionNumber = redirectPage.Substring(redirectPage.LastIndexOf("/v") + 2); // X.X.X
        }
        catch
        {
            throw new CouldNotCheckForUpdatesException();
        }

        if (Version.TryParse(latestVersionNumber, out var latestVersion))
        {
            return latestVersion;
        }

        throw new CouldNotCheckForUpdatesException();
    }
}
