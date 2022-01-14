namespace Bcdl.Downloader.Models;

public class Album
{
    public Album(string title, string artist, DateTime releaseDate, Track[] tracks)
    {
        Title = title;
        Artist = artist;
        ReleaseDate = releaseDate;
        Tracks = tracks;
    }

    public string Title { get; set; }
    public string Artist { get; set; }
    public DateTime ReleaseDate { get; set; }
    public Track[] Tracks { get; set; }
}