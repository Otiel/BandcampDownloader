using Newtonsoft.Json;

namespace BandcampDownloader.Bandcamp.Extraction.Dto;

internal sealed class JsonMp3File
{
    [JsonProperty("mp3-128")]
    public string Url { get; set; }
}
