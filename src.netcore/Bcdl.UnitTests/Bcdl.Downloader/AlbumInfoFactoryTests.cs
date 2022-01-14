using System;
using Bcdl.Downloader.Dto;
using Bcdl.Downloader.Factories;
using Bcdl.Downloader.Models;
using Moq;
using NUnit.Framework;

namespace Bcdl.UnitTests.Bcdl.Downloader;

public sealed class AlbumInfoFactoryTests
{
    private IAlbumInfoFactory _sut = null!;
    private Mock<IArtworkUrlBuilder> _artworkUrlBuilder = null!;
    private Mock<ITrackInfoFactory> _trackInfoFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _artworkUrlBuilder = new Mock<IArtworkUrlBuilder>();
        _trackInfoFactory = new Mock<ITrackInfoFactory>();

        _sut = new AlbumInfoFactory(_artworkUrlBuilder.Object, _trackInfoFactory.Object);
    }

    [Test]
    public void CreateAlbumInfo_should_return_null_artworkUrl_given_null_artworkId()
    {
        // Arrange
        var albumDto = new AlbumDto(
            new AlbumDataDto("title", DateTime.Now),
            null,
            "some artist",
            DateTime.Now,
            Array.Empty<TrackDto>());

        // Act
        var result = _sut.CreateAlbumInfo(albumDto, new LyricsDictionary());

        // Assert
        Assert.IsNull(result.ArtworkUrl);
    }

    [Test]
    public void CreateAlbumInfo_should_set_artworkUrl_given_non_null_artworkId()
    {
        // Arrange
        var artworkId = "artworkId";
        var artworkUrl = "artworkUrl";
        var albumDto = new AlbumDto(
            new AlbumDataDto("title", DateTime.Now),
            artworkId,
            "some artist",
            DateTime.Now,
            Array.Empty<TrackDto>());
        _artworkUrlBuilder.Setup(o => o.BuildArtworkUrl(artworkId)).Returns(artworkUrl);


        // Act
        var result = _sut.CreateAlbumInfo(albumDto, new LyricsDictionary());

        // Assert
        Assert.AreEqual(artworkUrl, result.ArtworkUrl);
    }
}
