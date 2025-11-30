using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BandcampDownloader.Bandcamp.Extraction;

internal interface IDiscographyService
{
    /// <summary>
    /// Retrieves all the albums URL existing on the specified Bandcamp page.
    /// </summary>
    /// <param name="htmlContent">The HTML source code of a Bandcamp page.</param>
    /// <returns>The albums URL existing on the specified Bandcamp page.</returns>
    IReadOnlyCollection<string> GetRelativeAlbumsUrlsFromArtistPage(string htmlContent);
}

internal sealed class DiscographyService : IDiscographyService
{
    public IReadOnlyCollection<string> GetRelativeAlbumsUrlsFromArtistPage(string htmlContent)
    {
        // Get albums ("real" albums or track-only pages) relative urls
        var regex = new Regex("href=\"(?<url>/(album|track)/.*)\"");
        if (!regex.IsMatch(htmlContent))
        {
            throw new NoAlbumFoundException();
        }

        var albumsUrl = new List<string>();
        foreach (Match m in regex.Matches(htmlContent))
        {
            albumsUrl.Add(m.Groups["url"].Value);
        }

        // Remove duplicates
        albumsUrl = albumsUrl.Distinct().ToList();
        return albumsUrl;
    }
}
