using Bcdl.Downloader.Models;

namespace Bcdl.Downloader.Services;

public class DownloadAlbumService
{
    private readonly IDownloadCoverArtService _downloadCoverArtService;
    private readonly IDownloadTrackService _downloadTrackService;

    public DownloadAlbumService(
        IDownloadCoverArtService downloadCoverArtService,
        IDownloadTrackService downloadTrackService)
    {
        _downloadCoverArtService = downloadCoverArtService;
        _downloadTrackService = downloadTrackService;
    }

    public async void DownloadAlbumAsync(AlbumInfo albumInfo, CancellationToken ct)
    {
        await _downloadCoverArtService.DownloadCoverArtAsync(albumInfo, ct).ConfigureAwait(false);

        foreach (var trackInfo in albumInfo.TrackInfos)
        {
            await _downloadTrackService.DownloadTrackAsync(trackInfo, ct).ConfigureAwait(false);
        }
    }
}
