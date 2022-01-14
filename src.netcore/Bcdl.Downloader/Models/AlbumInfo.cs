namespace Bcdl.Downloader.Models;

public sealed class AlbumInfo
{
    public AlbumInfo(string artist, string title, string? artworkUrl, DateOnly releaseDate, TrackInfo[] trackInfos)
    {
        Artist = artist;
        Title = title;
        ArtworkUrl = artworkUrl;
        ReleaseDate = releaseDate;
        TrackInfos = trackInfos;
    }

    public string Artist { get; }
    public string Title { get; }
    public string? ArtworkUrl { get; }
    public DateOnly ReleaseDate { get; }
    public TrackInfo[] TrackInfos { get; }
}