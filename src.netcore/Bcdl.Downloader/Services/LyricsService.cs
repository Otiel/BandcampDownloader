using System.Net;
using System.Text.RegularExpressions;

namespace Bcdl.Downloader.Services;

public interface ILyricsService
{
    string? ExtractLyrics(string albumHtml, int trackNumber);
}

public sealed class LyricsService : ILyricsService
{
    public string? ExtractLyrics(string albumHtml, int trackNumber)
    {
        var htmlTableRowId = $"lyrics_row_{trackNumber}";
        const string lyricsCapturingGroup = "lyrics";
        var regex = new Regex($"<tr.+id=\"{htmlTableRowId}\">.*?<div>(?<{lyricsCapturingGroup}>.*?)</div>.*</tr>", RegexOptions.Singleline);
        var match = regex.Match(albumHtml);

        if (!match.Success)
        {
            return null;
        }

        var capturedLyrics = match.Groups[lyricsCapturingGroup].Value;
        var lyrics = WebUtility.HtmlDecode(capturedLyrics);

        return lyrics;
    }
}
