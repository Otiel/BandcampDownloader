namespace Bcdl.Downloader.Services;

public interface IDownloadWebPageService
{
    Task<string> DownloadWebPageAsync(string url, CancellationToken ct);
}

public sealed class DownloadWebPageService : IDownloadWebPageService
{
    private readonly HttpClient _httpClient;

    public DownloadWebPageService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> DownloadWebPageAsync(string url, CancellationToken ct)
    {
        var webPage = await _httpClient.GetStringAsync(url, ct).ConfigureAwait(false);
        return webPage;
    }
}
