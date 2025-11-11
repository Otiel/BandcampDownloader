using System.Text.Json.Serialization;

namespace BandcampDownloader.Core.Updates;

public sealed class GithubRelease
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("tag_name")]
    public string TagName { get; init; }

    [JsonPropertyName("prerelease")]
    public bool IsPrerelease { get; init; }

    [JsonPropertyName("draft")]
    public bool IsDraft { get; init; }

    [JsonPropertyName("assets")]
    public GithubReleaseAsset[] Assets { get; init; }
}
