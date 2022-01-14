using Bcdl.Downloader.Dto;
using Bcdl.Downloader.Models;

namespace Bcdl.Downloader.Factories;

public interface ITrackInfoFactory
{
    TrackInfo CreateTrackInfo(TrackDto trackDto, string? lyrics);
}

public sealed class TrackInfoFactory : ITrackInfoFactory
{
    public TrackInfo CreateTrackInfo(TrackDto trackDto, string? lyrics)
    {
        return new TrackInfo(
            trackDto.Number,
            trackDto.Title,
            trackDto.Duration,
            lyrics,
            trackDto.FileDto.Url);
    }
}
