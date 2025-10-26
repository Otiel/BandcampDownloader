using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BandcampDownloader.Helpers;

namespace BandcampDownloader.Updates;

internal interface IUpdatesService
{
    /// <summary>
    /// Returns the latest version available.
    /// </summary>
    Task<Version> GetLatestVersionAsync();
}

internal sealed class UpdatesService : IUpdatesService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private const string LATEST_RELEASE_URL = "https://api.github.com/repos/otiel/bandcampdownloader/releases/latest";

    public UpdatesService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Version> GetLatestVersionAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BandcampDownloader");

        var latestRelease = await httpClient.GetFromJsonAsync<GithubRelease>(LATEST_RELEASE_URL);

        // BandcampDownloader tags are usually "vX.Y.Z"
        var latestVersionNumber = latestRelease.TagName.Replace("v", "");

        if (!Version.TryParse(latestVersionNumber, out var latestVersion))
        {
            throw new CouldNotCheckForUpdatesException();
        }

        return latestVersion;
    }
}
