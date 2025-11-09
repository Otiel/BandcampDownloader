using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global : used in deserialization

namespace BandcampDownloader.Updates;

// ReSharper disable once ClassNeverInstantiated.Global : used in deserialization
public sealed class GithubReleaseAsset
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; init; }
}
