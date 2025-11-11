using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Bandcamp.Extraction;
using BandcampDownloader.Helpers;
using BandcampDownloader.Net;

namespace BandcampDownloader.Bandcamp.Download;

internal interface IAlbumUrlRetriever
{
    Task<IReadOnlyList<string>> RetrieveAlbumsUrlsAsync(string inputUrls, bool downloadArtistDiscography, CancellationToken cancellationToken);
    event DownloadProgressChangedEventHandler DownloadProgressChanged;
}

internal sealed class AlbumUrlRetriever : IAlbumUrlRetriever
{
    private readonly IBandcampExtractionService _bandcampExtractionService;
    private readonly IHttpService _httpService;
    public event DownloadProgressChangedEventHandler DownloadProgressChanged;

    public AlbumUrlRetriever(IBandcampExtractionService bandcampExtractionService, IHttpService httpService)
    {
        _bandcampExtractionService = bandcampExtractionService;
        _httpService = httpService;
    }

    public async Task<IReadOnlyList<string>> RetrieveAlbumsUrlsAsync(string inputUrls, bool downloadArtistDiscography, CancellationToken cancellationToken)
    {
        var splitUrls = inputUrls.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries).ToList();
        var sanitizedUrls = splitUrls.Distinct().Select(o => o.Trim()).ToList();

        if (!downloadArtistDiscography)
        {
            return sanitizedUrls;
        }

        var albumsUrls = await GetArtistDiscographyAlbumsUrlsAsync(sanitizedUrls, cancellationToken);
        return albumsUrls;
    }

    /// <summary>
    /// Returns the artists discography from any URL (artist, album, track).
    /// </summary>
    private async Task<IReadOnlyList<string>> GetArtistDiscographyAlbumsUrlsAsync(IReadOnlyList<string> urls, CancellationToken cancellationToken)
    {
        var albumsUrls = new List<string>();

        foreach (var url in urls.Select(UrlHelper.GetHttpUrlIfNeeded))
        {
            cancellationToken.ThrowIfCancellationRequested();

            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Retrieving artist discography from {url}", DownloadProgressChangedLevel.Info));

            // Get artist "music" bandcamp page (http://artist.bandcamp.com/music)
            var regex = new Regex("https?://[^/]*");
            var artistPage = regex.Match(url).ToString();
            var artistMusicPage = UrlHelper.GetHttpUrlIfNeeded(artistPage + "/music");

            // Retrieve artist "music" page HTML source code
            string htmlCode;

            try
            {
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloading album info from url: {url}", DownloadProgressChangedLevel.VerboseInfo));
                var httpClient = _httpService.CreateHttpClient();
                htmlCode = await httpClient.GetStringAsync(artistMusicPage, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Could not retrieve data for {artistMusicPage}", DownloadProgressChangedLevel.Error));
                continue;
            }

            var count = albumsUrls.Count;
            try
            {
                var albumsUrl = _bandcampExtractionService.GetAlbumsUrl(htmlCode, artistPage);
                albumsUrls.AddRange(albumsUrl);
            }
            catch (NoAlbumFoundException)
            {
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"No referred album could be found on {artistMusicPage}. Try to uncheck the \"Download artist discography\" option", DownloadProgressChangedLevel.Error));
            }

            if (albumsUrls.Count - count == 0)
            {
                // This seems to be a one-album artist with no "music" page => URL redirects to the unique album URL
                albumsUrls.Add(url);
            }
        }

        return albumsUrls.Distinct().ToList();
    }
}
