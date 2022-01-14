namespace Bcdl.Downloader.Factories;

public interface IArtworkUrlBuilder
{
    string BuildArtworkUrl(string artworkId);
}

public sealed class ArtworkUrlBuilder : IArtworkUrlBuilder
{
    private const string ARTWORK_URL_START = "https://f4.bcbits.com/img/a";
    private const string ARTWORK_URL_END = "_0.jpg";

    public string BuildArtworkUrl(string artworkId)
    {
        var artworkPaddedId = artworkId.PadLeft(10, '0');
        var artworkUrl = $"{ARTWORK_URL_START}{artworkPaddedId}{ARTWORK_URL_END}";

        return artworkUrl;
    }
}
