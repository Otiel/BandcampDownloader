using Newtonsoft.Json;

namespace Bcdl.Downloader.Dto;

public sealed class AlbumDataDto
{
    public AlbumDataDto(string albumTitle, DateTime releaseDate)
    {
        AlbumTitle = albumTitle;
        ReleaseDate = releaseDate;
    }

    [JsonProperty("title")]
    public string AlbumTitle { get; set; }

    [JsonProperty("release_date")]
    public DateTime ReleaseDate { get; set; }
}
