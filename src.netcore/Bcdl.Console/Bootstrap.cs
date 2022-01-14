using Bcdl.Downloader.Factories;
using Bcdl.Downloader.Services;
using Serilog;
using SimpleInjector;

namespace Bcdl.Console;

internal static class Bootstrap
{
    private static readonly Container _container;

    static Bootstrap()
    {
        _container = new Container();
    }

    public static Container InitializeContainer()
    {
        RegisterTypes();
        _container.Verify();

        return _container;
    }

    private static void RegisterTypes()
    {
        _container.Register<IAlbumDtoService, AlbumDtoService>();
        _container.Register<IAlbumInfoFactory, AlbumInfoFactory>();
        _container.Register<IAlbumInfoService, AlbumInfoService>();
        _container.Register<IAlbumJsonService, AlbumJsonService>();
        _container.Register<IArtworkUrlBuilder, ArtworkUrlBuilder>();
        _container.Register<IDownloadFileService, DownloadFileService>();
        _container.Register<IDownloadWebPageService, DownloadWebPageService>(Lifestyle.Singleton);
        _container.Register<ILogger, Logger>(Lifestyle.Singleton);
        _container.Register<ILyricsService, LyricsService>();
        _container.Register<ITrackInfoFactory, TrackInfoFactory>();
    }
}
