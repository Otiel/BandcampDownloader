using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BandcampDownloader.Helpers;
using BandcampDownloader.Settings;

namespace BandcampDownloader.Net;

internal interface IHttpService
{
    /// <summary>
    /// Returns the size of the file located at the provided URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="protocolMethod">The protocol method to use in order to retrieve the file size.</param>
    /// <returns>The size of the file located at the provided URL.</returns>
    Task<long> GetFileSizeAsync(string url, string protocolMethod);

    /// <summary>
    /// Sets the proxy of the specified WebClient according to the UserSettings.
    /// </summary>
    /// <param name="webClient">The WebClient to modify.</param>
    void SetProxy(WebClient webClient);

    HttpClient CreateHttpClient();
}

internal sealed class HttpService : IHttpService
{
    private readonly ISettingsService _settingsService;
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpService(ISettingsService settingsService, IHttpClientFactory httpClientFactory)
    {
        _settingsService = settingsService;
        _httpClientFactory = httpClientFactory;
    }

    public HttpClient CreateHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BandcampDownloader");

        return httpClient;
    }

    public async Task<long> GetFileSizeAsync(string url, string protocolMethod)
    {
#pragma warning disable SYSLIB0014
        var webRequest = WebRequest.Create(UrlHelper.GetHttpUrlIfNeeded(url));
#pragma warning restore SYSLIB0014
        webRequest.Method = protocolMethod;

        long fileSize;
        try
        {
            using var webResponse = await webRequest.GetResponseAsync();
            fileSize = webResponse.ContentLength;
        }
        catch (Exception e)
        {
            throw new Exception("Could not retrieve file size.", e);
        }

        return fileSize;
    }

    public void SetProxy(WebClient webClient)
    {
        var userSettings = _settingsService.GetUserSettings();

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
