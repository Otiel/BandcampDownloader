using Bcdl.Downloader.Models;

namespace Bcdl.Downloader.Services;

public interface IDownloadCoverArtService
{
    Task DownloadCoverArtAsync(AlbumInfo albumInfo, CancellationToken ct);
}

public sealed class DownloadCoverArtService : IDownloadCoverArtService
{
    public async Task DownloadCoverArtAsync(AlbumInfo albumInfo, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
