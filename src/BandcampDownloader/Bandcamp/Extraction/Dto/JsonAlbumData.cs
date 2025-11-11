using System;
using Newtonsoft.Json;

namespace BandcampDownloader.Bandcamp.Extraction.Dto;

internal sealed class JsonAlbumData
{
    [JsonProperty("title")]
    public string AlbumTitle { get; set; }

    [JsonProperty("release_date")]
    public DateTime ReleaseDate { get; set; }
}
