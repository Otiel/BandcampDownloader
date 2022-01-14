namespace Bcdl.Downloader.Models;

internal interface ILyricsDictionary
{
    void AddLyrics(int trackNumber, string lyrics);
    bool TryGetLyrics(int trackNumber, out string? lyrics);
}

public sealed class LyricsDictionary : ILyricsDictionary
{
    private readonly Dictionary<int, string?> _lyricsDictionary;

    public LyricsDictionary()
    {
        _lyricsDictionary = new Dictionary<int, string?>();
    }

    public void AddLyrics(int trackNumber, string lyrics)
    {
        _lyricsDictionary.Add(trackNumber, lyrics);
    }

    public bool TryGetLyrics(int trackNumber, out string? lyrics)
    {
        return _lyricsDictionary.TryGetValue(trackNumber, out lyrics);
    }
}
