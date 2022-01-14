using Bcdl.Downloader.Factories;
using NUnit.Framework;

namespace Bcdl.UnitTests.Bcdl.Downloader;

public sealed class ArtworkUrlBuilderTests
{
    private IArtworkUrlBuilder _sut = null!;
    private const string ARTWORK_URL_START = "https://f4.bcbits.com/img/a";
    private const string ARTWORK_URL_END = "_0.jpg";

    [SetUp]
    public void SetUp()
    {
        _sut = new ArtworkUrlBuilder();
    }

    [Test]
    public void BuildArtworkUrl_should_return_correct_values_given_one_character_artworkId()
    {
        // Arrange
        var artworkId = "1";
        var artworkUrl = $"{ARTWORK_URL_START}000000000{artworkId}{ARTWORK_URL_END}";

        // Act
        var result = _sut.BuildArtworkUrl(artworkId);

        // Assert
        Assert.AreEqual(artworkUrl, result);
    }

    [Test]
    public void BuildArtworkUrl_should_return_correct_values_given_two_character_artworkId()
    {
        // Arrange
        var artworkId = "12";
        var artworkUrl = $"{ARTWORK_URL_START}00000000{artworkId}{ARTWORK_URL_END}";

        // Act
        var result = _sut.BuildArtworkUrl(artworkId);

        // Assert
        Assert.AreEqual(artworkUrl, result);
    }

    [Test]
    public void BuildArtworkUrl_should_return_correct_values_given_nine_character_artworkId()
    {
        // Arrange
        var artworkId = "123456789";
        var artworkUrl = $"{ARTWORK_URL_START}0{artworkId}{ARTWORK_URL_END}";

        // Act
        var result = _sut.BuildArtworkUrl(artworkId);

        // Assert
        Assert.AreEqual(artworkUrl, result);
    }

    [Test]
    public void BuildArtworkUrl_should_return_correct_values_given_ten_character_artworkId()
    {
        // Arrange
        var artworkId = "1234567890";
        var artworkUrl = $"{ARTWORK_URL_START}{artworkId}{ARTWORK_URL_END}";

        // Act
        var result = _sut.BuildArtworkUrl(artworkId);

        // Assert
        Assert.AreEqual(artworkUrl, result);
    }

    [Test]
    public void BuildArtworkUrl_should_return_correct_values_given_eleven_character_artworkId()
    {
        // Arrange
        var artworkId = "12345678901";
        var artworkUrl = $"{ARTWORK_URL_START}{artworkId}{ARTWORK_URL_END}";

        // Act
        var result = _sut.BuildArtworkUrl(artworkId);

        // Assert
        Assert.AreEqual(artworkUrl, result);
    }
}
