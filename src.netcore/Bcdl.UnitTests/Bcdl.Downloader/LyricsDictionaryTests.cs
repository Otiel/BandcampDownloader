using Bcdl.Downloader.Models;
using NUnit.Framework;

namespace Bcdl.UnitTests.Bcdl.Downloader;

public sealed class LyricsDictionaryTests
{
    private ILyricsDictionary _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new LyricsDictionary();
    }

    [Test]
    public void TryGetLyrics_should_return_null_given_empty_dictionary()
    {
        // Act
        var result = _sut.TryGetLyrics(1, out var lyricsResult);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(lyricsResult);
    }

    [Test]
    public void AddLyrics_then_TryGetLyrics_should_return_lyrics_given_known_track_number()
    {
        // Arrange
        var lyrics = "some lyrics";
        var trackNumber = 1;

        // Act
        _sut.AddLyrics(trackNumber, lyrics);
        var result = _sut.TryGetLyrics(trackNumber, out var lyricsResult);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(lyrics, lyricsResult);
    }

    [Test]
    public void AddLyrics_then_TryGetLyrics_should_return_null_given_unknown_track_number()
    {
        // Arrange
        var lyrics = "some lyrics";
        var trackNumber = 1;

        // Act
        _sut.AddLyrics(trackNumber, lyrics);
        var result = _sut.TryGetLyrics(trackNumber + 1, out var lyricsResult);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(lyricsResult);
    }
}
