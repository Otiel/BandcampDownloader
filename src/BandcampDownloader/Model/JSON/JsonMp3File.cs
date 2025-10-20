using Newtonsoft.Json;

namespace BandcampDownloader.Model.JSON;

internal class JsonMp3File
{
    [JsonProperty("mp3-128")]
    public string Url { get; set; }
}
