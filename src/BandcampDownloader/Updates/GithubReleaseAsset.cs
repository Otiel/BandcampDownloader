using System.Text.Json.Serialization;

namespace BandcampDownloader.Updates;

// ReSharper disable once ClassNeverInstantiated.Global : use in deserialization
public sealed class GithubReleaseAsset
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; init; }
}
