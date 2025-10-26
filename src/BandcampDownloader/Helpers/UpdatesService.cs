using System;
using System.Net;
using System.Threading.Tasks;
using BandcampDownloader.Core;

namespace BandcampDownloader.Helpers;

internal interface IUpdatesService
{
    /// <summary>
    /// Returns the latest version available.
    /// </summary>
    Task<Version> GetLatestVersionAsync();
}

internal sealed class UpdatesService : IUpdatesService
{
    public async Task<Version> GetLatestVersionAsync()
    {
        // Note: GitHub uses a HTTP redirect to redirect from the generic latest release page to the actual latest release page

        // Retrieve the redirect page from the GitHub latest release page
#pragma warning disable SYSLIB0014
        var request = WebRequest.CreateHttp(Constants.UrlLatestRelease);
#pragma warning restore SYSLIB0014
        request.AllowAutoRedirect = false;
        string redirectPage;
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
            latestVersionNumber = redirectPage.Substring(redirectPage.LastIndexOf("/v", StringComparison.Ordinal) + 2); // X.X.X
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
