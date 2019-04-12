using System;
using System.Net;

namespace BandcampDownloader {

    internal static class UpdatesHelper {

        /// <summary>
        /// Returns the latest version available.
        /// </summary>
        public static Version GetLatestVersion() {
            // Note: GitHub uses a HTTP redirect to redirect from the generic latest release page to the actual latest release page

            // Retrieve the redirect page from the GitHub latest release page
            var request = HttpWebRequest.CreateHttp(Constants.LatestReleaseWebsite);
            request.AllowAutoRedirect = false;
            String redirectPage = "";
            try {
                using (var response = (HttpWebResponse) request.GetResponse()) {
                    redirectPage = response.GetResponseHeader("Location");
                    // redirectPage should be like "https://github.com/Otiel/BandcampDownloader/releases/tag/vX.X.X.X"
                }
            } catch {
                throw new CouldNotCheckForUpdatesException();
            }

            // Extract the version number from the URL
            String latestVersionNumber = "";
            try {
                latestVersionNumber = redirectPage.Substring(redirectPage.LastIndexOf("/v") + 2); // X.X.X.X
            } catch {
                throw new CouldNotCheckForUpdatesException();
            }

            if (Version.TryParse(latestVersionNumber, out Version latestVersion)) {
                return latestVersion;
            } else {
                throw new CouldNotCheckForUpdatesException();
            }
        }
    }
}