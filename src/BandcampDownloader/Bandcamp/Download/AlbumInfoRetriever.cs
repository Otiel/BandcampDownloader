using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Bandcamp.Extraction;
using BandcampDownloader.Model;
using BandcampDownloader.Net;
using NLog;

namespace BandcampDownloader.Bandcamp.Download;

internal interface IAlbumInfoRetriever
{
    Task<IReadOnlyList<Album>> GetAlbumsAsync(IReadOnlyList<string> albumsUrls, CancellationToken cancellationToken);
    event DownloadProgressChangedEventHandler DownloadProgressChanged;
}

internal sealed class AlbumInfoRetriever : IAlbumInfoRetriever
{
    private readonly IBandcampExtractionService _bandcampExtractionService;
    private readonly IHttpService _httpService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    public event DownloadProgressChangedEventHandler DownloadProgressChanged;

    public AlbumInfoRetriever(IBandcampExtractionService bandcampExtractionService, IHttpService httpService)
    {
        _bandcampExtractionService = bandcampExtractionService;
        _httpService = httpService;
    }

    public async Task<IReadOnlyList<Album>> GetAlbumsAsync(IReadOnlyList<string> albumsUrls, CancellationToken cancellationToken)
    {
        var albums = new List<Album>();

        foreach (var url in albumsUrls)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Retrieving album data for {url}", DownloadProgressChangedLevel.Info));

            // Retrieve URL HTML source code
            string htmlContent;
            try
            {
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Downloading album info from url: {url}", DownloadProgressChangedLevel.VerboseInfo));
                var httpClient = _httpService.CreateHttpClient();
                htmlContent = await httpClient.GetStringAsync(url, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(ex, $"Error downloading album info from url: {url}");
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Could not retrieve data for {url}", DownloadProgressChangedLevel.Error));
                continue;
            }

            // Get info on album
            try
            {
                var album = _bandcampExtractionService.GetAlbumInfoFromAlbumPage(htmlContent, cancellationToken);

                if (album.Tracks.Count > 0)
                {
                    albums.Add(album);
                }
                else
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"No tracks found for {url}, album will not be downloaded", DownloadProgressChangedLevel.Warning));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(ex, $"Could not retrieve album info for {url}");
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Could not retrieve album info for {url}", DownloadProgressChangedLevel.Error));
            }
        }

        return albums;
    }
}
