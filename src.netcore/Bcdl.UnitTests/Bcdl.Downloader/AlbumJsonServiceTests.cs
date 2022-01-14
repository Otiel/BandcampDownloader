using System;
using System.Net;
using Bcdl.Downloader.Services;
using Moq;
using NUnit.Framework;
using Serilog;

namespace Bcdl.UnitTests.Bcdl.Downloader;

public sealed class AlbumJsonServiceTests
{
    private IAlbumJsonService _sut = null!;
    private const string START_STRING = "data-tralbum=\"{";
    private const string END_STRING = "}\"";

    [SetUp]
    public void SetUp()
    {
        var logger = Mock.Of<ILogger>();

        _sut = new AlbumJsonService(logger);
    }

    [Test]
    public void ExtractAlbumJson_should_throw_exception_given_start_string_not_found()
    {
        // Arrange
        var albumHtml = $"missing correct start string{END_STRING}";

        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            // Act
            _sut.ExtractAlbumJson(albumHtml);
        });
    }

    [Test]
    public void ExtractAlbumJson_should_throw_exception_given_end_string_not_found()
    {
        // Arrange
        var albumHtml = $"{START_STRING}missing correct end string";

        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            // Act
            _sut.ExtractAlbumJson(albumHtml);
        });
    }

    [Test]
    public void ExtractAlbumJson_should_return_json_content()
    {
        // Arrange
        var jsonContent = "some json";
        var albumHtml = $"{START_STRING}{jsonContent}{END_STRING}";

        // Act
        var result = _sut.ExtractAlbumJson(albumHtml);

        // Assert
        var json = $"{{{jsonContent}}}";
        Assert.AreEqual(json, result);
    }

    [Test]
    public void ExtractAlbumJson_should_decode_html_encoded_characters()
    {
        // Arrange
        var jsonContent = "some json with html encoded characters \"'<>&ⓒⓡ";
        var htmlEncodedJsonContent = WebUtility.HtmlEncode(jsonContent);
        var albumHtml = $"{START_STRING}{htmlEncodedJsonContent}{END_STRING}";

        // Act
        var result = _sut.ExtractAlbumJson(albumHtml);

        // Assert
        var json = $"{{{jsonContent}}}";
        Assert.AreEqual(json, result);
    }

    [Test]
    public void ExtractAlbumJson_should_search_for_end_string_after_start_string()
    {
        // Arrange
        var jsonContent = "some json";
        var albumHtml = $"{END_STRING}{START_STRING}{jsonContent}{END_STRING}";

        // Act
        var result = _sut.ExtractAlbumJson(albumHtml);

        // Assert
        var json = $"{{{jsonContent}}}";
        Assert.AreEqual(json, result);
    }

    [Test]
    public void ExtractAlbumJson_should_search_first_occurence_of_end_string()
    {
        // Arrange
        var jsonContent = "some json";
        var otherContent = "some other content";
        var albumHtml = $"{START_STRING}{jsonContent}{END_STRING}{otherContent}{END_STRING}";

        // Act
        var result = _sut.ExtractAlbumJson(albumHtml);

        // Assert
        var json = $"{{{jsonContent}}}";
        Assert.AreEqual(json, result);
    }
}
