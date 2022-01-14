namespace Bcdl.Downloader.Models;

public sealed class TrackInfo
{
    public TrackInfo(int number, string title, double duration, string? lyrics, string mp3Url)
    {
        Number = number;
        Title = title;
        Duration = duration;
        Lyrics = lyrics;
        Mp3Url = mp3Url;
    }

    public int Number { get; }
    public string Title { get; }
    public double Duration { get; }
    public string? Lyrics { get; }
    public string Mp3Url { get; }
}