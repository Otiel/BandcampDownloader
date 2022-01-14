using Bcdl.Downloader.Models;

namespace Bcdl.Downloader.Services;

public interface IDownloadTrackService
{
    Task DownloadTrackAsync(TrackInfo trackInfo, CancellationToken ct);
}

public sealed class DownloadTrackService : IDownloadTrackService
{
    public async Task DownloadTrackAsync(TrackInfo trackInfo, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
