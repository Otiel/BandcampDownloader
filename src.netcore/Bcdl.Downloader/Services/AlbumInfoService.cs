using Bcdl.Downloader.Dto;
using Bcdl.Downloader.Factories;
using Bcdl.Downloader.Models;

namespace Bcdl.Downloader.Services;

public interface IAlbumInfoService
{
    Task<AlbumInfo> DownloadAlbumInfoAsync(string albumUrl, CancellationToken ct);
}

public sealed class AlbumInfoService : IAlbumInfoService
{
    private readonly IAlbumInfoFactory _albumInfoFactory;
    private readonly IDownloadWebPageService _downloadWebPageService;
    private readonly IAlbumDtoService _albumDtoService;
    private readonly ILyricsService _lyricsService;

    public AlbumInfoService(
        IAlbumInfoFactory albumInfoFactory,
        IDownloadWebPageService downloadWebPageService,
        IAlbumDtoService albumDtoService,
        ILyricsService lyricsService)
    {
        _albumInfoFactory = albumInfoFactory;
        _downloadWebPageService = downloadWebPageService;
        _albumDtoService = albumDtoService;
        _lyricsService = lyricsService;
    }

    public async Task<AlbumInfo> DownloadAlbumInfoAsync(string albumUrl, CancellationToken ct)
    {
        var albumWebPage = await _downloadWebPageService.DownloadWebPageAsync(albumUrl, ct).ConfigureAwait(false);
        var albumInfo = ExtractAlbumInfo(albumWebPage);
        return albumInfo;
    }

    private AlbumInfo ExtractAlbumInfo(string albumHtml)
    {
        var albumDto = _albumDtoService.ExtractAlbumDto(albumHtml);
        var lyricsDictionary = ExtractLyrics(albumHtml, albumDto.Tracks);
        var albumInfo = _albumInfoFactory.CreateAlbumInfo(albumDto, lyricsDictionary);

        return albumInfo;
    }

    private LyricsDictionary ExtractLyrics(string albumHtml, IEnumerable<TrackDto> trackDtos)
    {
        var lyricsDictionary = new LyricsDictionary();
        foreach (var trackDto in trackDtos)
        {
            var lyrics = _lyricsService.ExtractLyrics(albumHtml, trackDto.Number);
            if (lyrics is not null)
            {
                lyricsDictionary.AddLyrics(trackDto.Number, lyrics);
            }
        }

        return lyricsDictionary;
    }
}