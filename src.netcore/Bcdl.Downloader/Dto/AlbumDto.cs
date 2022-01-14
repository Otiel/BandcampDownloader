using Newtonsoft.Json;

namespace Bcdl.Downloader.Dto;

public sealed class AlbumDto
{
    public AlbumDto(AlbumDataDto albumDataDto, string? artworkId, string artist, DateTime releaseDate, TrackDto[] tracks)
    {
        AlbumDataDto = albumDataDto;
        ArtworkId = artworkId;
        Artist = artist;
        ReleaseDate = releaseDate;
        Tracks = tracks;
    }

    [JsonProperty("current")]
    public AlbumDataDto AlbumDataDto { get; set; }

    [JsonProperty("art_id")]
    public string? ArtworkId { get; set; }

    [JsonProperty("artist")]
    public string Artist { get; set; }

    [JsonProperty("album_release_date")]
    public DateTime ReleaseDate { get; set; }

    [JsonProperty("trackinfo")]
    public TrackDto[] Tracks { get; set; }
}