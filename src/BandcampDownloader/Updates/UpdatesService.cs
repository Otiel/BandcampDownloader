using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BandcampDownloader.Net;

namespace BandcampDownloader.Updates;

internal interface IUpdatesService
{
    Task<Version> GetLatestVersionAsync();
    Task<string> GetLatestReleaseAssetUrlAsync();
}

internal sealed class UpdatesService : IUpdatesService
{
    private readonly IHttpService _httpService;

    private const string LATEST_RELEASE_URL = "https://api.github.com/repos/otiel/bandcampdownloader/releases/latest";
    private const string RELEASE_ASSET_NAME = "BandcampDownloader.zip";

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

    public async Task<string> GetLatestReleaseAssetUrlAsync()
    {
        var httpClient = _httpService.CreateHttpClient();
        var latestRelease = await httpClient.GetFromJsonAsync<GithubRelease>(LATEST_RELEASE_URL);
        var latestReleaseAsset = latestRelease.Assets.Single(x => x.Name == RELEASE_ASSET_NAME);
        return latestReleaseAsset.DownloadUrl;
    }
}
