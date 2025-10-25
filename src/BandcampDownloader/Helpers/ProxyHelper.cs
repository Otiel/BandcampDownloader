using System;
using System.Net;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Settings;

namespace BandcampDownloader.Helpers;

internal static class ProxyHelper
{
    /// <summary>
    /// Sets the proxy of the specified WebClient according to the UserSettings.
    /// </summary>
    /// <param name="webClient">The WebClient to modify.</param>
    public static void SetProxy(WebClient webClient)
    {
        var userSettings = DependencyInjectionHelper.GetService<ISettingsService>().GetUserSettings();

        switch (userSettings.Proxy)
        {
            case ProxyType.None:
                webClient.Proxy = null;
                break;
            case ProxyType.System:
                if (webClient.Proxy != null)
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                break;
            case ProxyType.Manual:
                webClient.Proxy = new WebProxy(userSettings.ProxyHttpAddress, userSettings.ProxyHttpPort);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
