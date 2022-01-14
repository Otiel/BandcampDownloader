using System.Diagnostics;
using Bcdl.Downloader.Dto;
using Newtonsoft.Json;

namespace Bcdl.Downloader.Services;

public interface IAlbumDtoService
{
    AlbumDto ExtractAlbumDto(string albumHtml);
}

public sealed class AlbumDtoService : IAlbumDtoService
{
    private readonly IAlbumJsonService _albumJsonService;

    public AlbumDtoService(IAlbumJsonService albumJsonService)
    {
        _albumJsonService = albumJsonService;
    }

    public AlbumDto ExtractAlbumDto(string albumHtml)
    {
        var json = _albumJsonService.ExtractAlbumJson(albumHtml);

        var albumDto = JsonConvert.DeserializeObject<AlbumDto>(json);
        Debug.Assert(albumDto != null, $"{nameof(albumDto)} != null");

        return albumDto;
    }
}
