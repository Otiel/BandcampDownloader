using Newtonsoft.Json;

namespace Bcdl.Downloader.Dto;

public sealed class Mp3FileDto
{
    public Mp3FileDto(string url)
    {
        Url = url;
    }

    [JsonProperty("mp3-128")]
    public string Url { get; set; }
}
