using System;
using System.Text.Json.Serialization;

namespace BandcampDownloader.Bandcamp.Extraction.Dto;

internal sealed class JsonAlbumData
{
    [JsonPropertyName("title")]
    public string AlbumTitle { get; set; }

    [JsonPropertyName("release_date")]
    public DateTime? ReleaseDate { get; set; }

    [JsonPropertyName("publish_date")]
    public DateTime? PublishDate { get; set; }
}
