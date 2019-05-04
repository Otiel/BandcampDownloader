using System;
using System.Net;

namespace BandcampDownloader {

    internal class ProxyHelper {

        /// <summary>
        /// Sets the proxy of the specified WebClient according to the UserSettings.
        /// </summary>
        /// <param name="webClient">The WebClient to modify.</param>
        public static void SetProxy(WebClient webClient) {
            switch (App.UserSettings.Proxy) {
                case ProxyType.None:
                    webClient.Proxy = null;
                    break;
                case ProxyType.System:
                    if (webClient.Proxy != null) {
                        webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }
                    break;
                case ProxyType.Manual:
                    webClient.Proxy = new WebProxy(App.UserSettings.ProxyHttpAddress, App.UserSettings.ProxyHttpPort);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}