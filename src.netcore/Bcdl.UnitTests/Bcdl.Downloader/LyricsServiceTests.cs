using System.Net;
using Bcdl.Downloader.Services;
using NUnit.Framework;

namespace Bcdl.UnitTests.Bcdl.Downloader;

public sealed class LyricsServiceTests
{
    private ILyricsService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new LyricsService();
    }

    [Test]
    public void ExtractLyrics_should_return_null_given_no_lyrics()
    {
        var result = _sut.ExtractLyrics("Fake HTML", 0);

        Assert.AreEqual(null, result);
    }

    [Test]
    public void ExtractLyrics_should_return_lyrics_given_simple_lyrics_and_single_line_html()
    {
        var trackNumber = 1;
        var lyrics = CreateSimpleLyrics();
        var html = CreateSingleLineHtml(trackNumber, lyrics);

        var result = _sut.ExtractLyrics(html, trackNumber);

        Assert.AreEqual(lyrics, result);
    }

    [Test]
    public void ExtractLyrics_should_return_lyrics_given_html_encoded_lyrics_and_single_line_html()
    {
        var trackNumber = 1;
        var (htmlEncodedLyrics, htmlDecodedLyrics) = CreateHtmlEncodedLyrics();
        var html = CreateSingleLineHtml(trackNumber, htmlEncodedLyrics);

        var result = _sut.ExtractLyrics(html, trackNumber);

        Assert.AreEqual(htmlDecodedLyrics, result);
    }

    [Test]
    public void ExtractLyrics_should_return_lyrics_given_simple_lyrics_and_multi_line_html()
    {
        var trackNumber = 1;
        var lyrics = CreateSimpleLyrics();
        var html = CreateMultiLineHtml(trackNumber, lyrics);

        var result = _sut.ExtractLyrics(html, trackNumber);

        Assert.AreEqual(lyrics, result);
    }

    [Test]
    public void ExtractLyrics_should_return_lyrics_given_html_encoded_lyrics_and_multi_line_html()
    {
        var trackNumber = 1;
        var (htmlEncodedLyrics, htmlDecodedLyrics) = CreateHtmlEncodedLyrics();
        var html = CreateMultiLineHtml(trackNumber, htmlEncodedLyrics);

        var result = _sut.ExtractLyrics(html, trackNumber);

        Assert.AreEqual(htmlDecodedLyrics, result);
    }

    [Test]
    public void ExtractLyrics_should_return_correct_lyrics_given_multiple_lyrics()
    {
        var trackNumber = 1;
        var lyrics1 = "lyrics 1";
        var lyrics2 = "lyrics 2";
        var html1 = CreateMultiLineHtml(trackNumber, lyrics1);
        var html2 = CreateMultiLineHtml(trackNumber + 1, lyrics2);
        var html = html1 + html2;

        var result = _sut.ExtractLyrics(html, trackNumber);

        Assert.AreEqual(lyrics1, result);
    }

    private static string CreateSimpleLyrics()
    {
        return @"Some simple lyrics
            Containing no html encoded characters
            Some verse";
    }

    private static (string htmlEncodedLyrics, string htmlDecodedLyrics) CreateHtmlEncodedLyrics()
    {
        var htmlDecodedLyrics = @"Some lyrics
            Containing some HTML encoded characters
            ""'<>&ⓒⓡ
            Some verse";
        var htmlEncodedLyrics = WebUtility.HtmlEncode(htmlDecodedLyrics);
        return (htmlEncodedLyrics, htmlDecodedLyrics);
    }

    private static string CreateSingleLineHtml(int trackNumber, string lyrics)
    {
        return $@"<tr class=""lyricsRow"" id=""lyrics_row_{trackNumber}""><td></td><td colspan=4><div>{lyrics}</div></td></tr>";
    }

    private static string CreateMultiLineHtml(int trackNumber, string lyrics)
    {
        return $@"                    <tr class=""lyricsRow"" id=""lyrics_row_{trackNumber}"">
            <td></td>
            <td colspan=4>
            <div>{lyrics}</div>
            </td>
            </tr>";
    }
}
