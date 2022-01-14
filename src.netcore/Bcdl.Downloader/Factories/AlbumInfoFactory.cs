using Bcdl.Downloader.Dto;
using Bcdl.Downloader.Models;

namespace Bcdl.Downloader.Factories;

public interface IAlbumInfoFactory
{
    AlbumInfo CreateAlbumInfo(AlbumDto albumDto, LyricsDictionary lyricsDictionary);
}

public sealed class AlbumInfoFactory : IAlbumInfoFactory
{
    private readonly IArtworkUrlBuilder _artworkUrlBuilder;
    private readonly ITrackInfoFactory _trackInfoFactory;

    public AlbumInfoFactory(
        IArtworkUrlBuilder artworkUrlBuilder,
        ITrackInfoFactory trackInfoFactory)
    {
        _artworkUrlBuilder = artworkUrlBuilder;
        _trackInfoFactory = trackInfoFactory;
    }

    public AlbumInfo CreateAlbumInfo(AlbumDto albumDto, LyricsDictionary lyricsDictionary)
    {
        var trackInfos = new List<TrackInfo>();
        foreach (var trackDto in albumDto.Tracks)
        {
            lyricsDictionary.TryGetLyrics(trackDto.Number, out var lyrics);
            var trackInfo = _trackInfoFactory.CreateTrackInfo(trackDto, lyrics);
            trackInfos.Add(trackInfo);
        }

        var artworkUrl = albumDto.ArtworkId != null ? _artworkUrlBuilder.BuildArtworkUrl(albumDto.ArtworkId) : null;

        return new AlbumInfo(
            albumDto.Artist,
            albumDto.AlbumDataDto.AlbumTitle,
            artworkUrl,
            DateOnly.FromDateTime(albumDto.ReleaseDate),
            trackInfos.ToArray());
    }
}
