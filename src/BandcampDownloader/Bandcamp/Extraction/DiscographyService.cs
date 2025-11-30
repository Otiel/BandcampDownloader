using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace BandcampDownloader.Bandcamp.Extraction;

internal interface IDiscographyService
{
    /// <summary>
    /// Returns all the albums URLs existing on the specified "/music" Bandcamp page.
    /// </summary>
    /// <param name="musicPageHtmlContent">The HTML source code of the "/music" Bandcamp page of an artist.</param>
    IReadOnlyCollection<string> GetReferredAlbumsRelativeUrls(string musicPageHtmlContent);
}

internal sealed class DiscographyService : IDiscographyService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public IReadOnlyCollection<string> GetReferredAlbumsRelativeUrls(string musicPageHtmlContent)
    {
        if (IsSingleAlbumArtist(musicPageHtmlContent))
        {
            _logger.Info("Found single album artist when looking for its discography");

            var albumUrl = GetAlbumUrlFromSingleAlbumArtist(musicPageHtmlContent);

            _logger.Info($"Found album for a single album artist with the following relative URL: {albumUrl}");

            return [albumUrl];
        }

        var regex = new Regex("(?<url>/(album|track)/.+?)(\"|&quot;|[?])");
        if (!regex.IsMatch(musicPageHtmlContent))
        {
            throw new NoAlbumFoundException();
        }

        var albumsUrl = new List<string>();
        foreach (Match m in regex.Matches(musicPageHtmlContent))
        {
            albumsUrl.Add(m.Groups["url"].Value);
        }

        // Remove duplicates
        var distinctAlbumsUrl = albumsUrl.Distinct().ToList();

        return distinctAlbumsUrl;
    }

    private static bool IsSingleAlbumArtist(string musicPageHtmlContent)
    {
        // When the artist has a single album, his music page often redirects to his album page
        // We look for 'div id="discography"' to determine if we're on an album page
        return musicPageHtmlContent.Contains("div id=\"discography\"");
    }

    private string GetAlbumUrlFromSingleAlbumArtist(string musicPageHtmlContent)
    {
        var regex = new Regex("href=\"(?<url>/album/.+?)\"");
        var matches = regex.Matches(musicPageHtmlContent);
        var albumUrls = matches.Select(m => m.Groups["url"].Value);
        var distinctAlbumUrls = albumUrls.Distinct().ToList();

        switch (distinctAlbumUrls.Count)
        {
            case 0:
                _logger.Error("No album URL found whereas we expected to find exactly one");
                throw new NoAlbumFoundException();
            case 1:
                return distinctAlbumUrls.Single();
            default:
                _logger.Error("Found multiple album URLs whereas we expected to find exactly one");
                throw new NoAlbumFoundException();
        }
    }
}
