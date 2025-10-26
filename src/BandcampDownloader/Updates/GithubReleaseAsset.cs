using System.Text.Json.Serialization;

namespace BandcampDownloader.Updates;

public sealed class GithubReleaseAsset
{
    [JsonPropertyName("url")]
    public string Url { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; init; }
}
