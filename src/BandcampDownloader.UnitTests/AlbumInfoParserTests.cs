using BandcampDownloader.Bandcamp.Extraction;
using BandcampDownloader.UnitTests.Resources;

namespace BandcampDownloader.UnitTests;

public sealed class AlbumInfoParserTests
{
    private AlbumInfoParser _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new AlbumInfoParser();
    }

    [Test]
    public void GetAlbumInfoFromAlbumPage_Returns_Expected_Given_Ifellinlovewiththestars2()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Track_Ifellinlovewiththestars2_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumInfo = _sut.GetAlbumInfoFromAlbumPage(htmlContent, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            // Assert.That(albumInfo.Title, );
            // Assert.That(albumInfo.Artist, );
            // Assert.That(albumInfo.ReleaseDate, );
            // Assert.That(albumInfo.HasArtwork, );
            // Assert.That(albumInfo.ArtworkPath, );
            // Assert.That(albumInfo.ArtworkUrl, );
            // Assert.That(albumInfo.Path, );
            // Assert.That(albumInfo.PlaylistPath, );
            // Assert.That(albumInfo.Tracks, );
        }
    }
}
