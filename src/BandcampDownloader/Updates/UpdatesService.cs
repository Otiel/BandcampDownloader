using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BandcampDownloader.Net;

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
    private readonly IHttpService _httpService;

    private const string LATEST_RELEASE_URL = "https://api.github.com/repos/otiel/bandcampdownloader/releases/latest";

    public UpdatesService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<Version> GetLatestVersionAsync()
    {
        var httpClient = _httpService.CreateHttpClient();

        var latestRelease = await httpClient.GetFromJsonAsync<GithubRelease>(LATEST_RELEASE_URL);

        // BandcampDownloader tags are usually "vX.Y.Z"
        var latestVersionNumber = latestRelease.TagName.Replace("v", "");

        var latestVersion = Version.Parse(latestVersionNumber);

        return latestVersion;
    }
}
