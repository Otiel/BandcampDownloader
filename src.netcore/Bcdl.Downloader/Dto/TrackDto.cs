using Newtonsoft.Json;

namespace Bcdl.Downloader.Dto;

public sealed class TrackDto
{
    public TrackDto(double duration, Mp3FileDto fileDto, int number, string title)
    {
        Duration = duration;
        FileDto = fileDto;
        Number = number;
        Title = title;
    }

    [JsonProperty("duration")]
    public double Duration { get; set; }

    [JsonProperty("file")]
    public Mp3FileDto FileDto { get; set; }

    [JsonProperty("track_num")]
    public int Number { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
}
