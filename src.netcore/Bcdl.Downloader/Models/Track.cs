namespace Bcdl.Downloader.Models;

public class Track
{
    public Track(int number, string title, string artist, string lyrics)
    {
        Number = number;
        Title = title;
        Artist = artist;
        Lyrics = lyrics;
    }

    public int Number { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Lyrics { get; set; }
}