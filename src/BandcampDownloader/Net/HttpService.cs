using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Settings;
using NLog;

namespace BandcampDownloader.Net;

internal interface IHttpService
{
    HttpClient CreateHttpClient();
    Task<long> GetFileSizeAsync(string url, CancellationToken cancellationToken);
    Task DownloadFileAsync(string url, FileInfo fileInfo, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the proxy of the specified WebClient according to the UserSettings.
    /// </summary>
    /// <param name="webClient">The WebClient to modify.</param>
    void SetProxy(WebClient webClient);
}

internal sealed class HttpService : IHttpService
{
    private readonly ISettingsService _settingsService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public HttpService(ISettingsService settingsService, IHttpClientFactory httpClientFactory)
    {
        _settingsService = settingsService;
        _httpClientFactory = httpClientFactory;
    }

    public HttpClient CreateHttpClient()
    {
        return CreateHttpClientInternal();
    }

    public async Task<long> GetFileSizeAsync(string url, CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClientInternal();
        var request = new HttpRequestMessage(HttpMethod.Head, url); // Use HEAD method in order to retrieve only headers
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var fileSize = response.Content.Headers.ContentLength;

        if (fileSize == null)
        {
            _logger.Error($"No Content-Length header found for {url}");
            throw new Exception();
        }

        return fileSize.Value;
    }

    public async Task DownloadFileAsync(string url, FileInfo fileInfo, CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClientInternal();
        await using var httpStream = await httpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
        await using var fileStream = File.Create(fileInfo.FullName);
        await httpStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
    }

    public void SetProxy(WebClient webClient) // TODO fix unused
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

    private HttpClient CreateHttpClientInternal()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestVersion = HttpVersion.Version20;
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BandcampDownloader");

        return httpClient;
    }
}
