using BandcampDownloader.Audio;
using BandcampDownloader.Bandcamp.Download;
using BandcampDownloader.Bandcamp.Extraction;
using BandcampDownloader.Core.Localization;
using BandcampDownloader.Core.Logging;
using BandcampDownloader.Core.Themes;
using BandcampDownloader.Core.Updates;
using BandcampDownloader.Helpers;
using BandcampDownloader.IO;
using BandcampDownloader.Net;
using BandcampDownloader.Settings;
using BandcampDownloader.UI.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace BandcampDownloader.Core.DependencyInjection;

/// <summary>
/// Wrapper around the underlying dependency injection container.
/// </summary>
internal interface IContainer
{
    T GetService<T>();
}

internal sealed class Container : IContainer
{
    private readonly ServiceProvider _serviceProvider;

    public Container()
    {
        var serviceCollection = GetServiceCollection();
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public T GetService<T>()
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddHttpClient();
        serviceCollection.AddSingleton<IAlbumInfoRetriever, AlbumInfoRetriever>();
        serviceCollection.AddSingleton<IAlbumUrlRetriever, AlbumUrlRetriever>();
        serviceCollection.AddSingleton<IBandcampExtractionService, BandcampExtractionService>();
        serviceCollection.AddSingleton<IDownloadManager, DownloadManager>();
        serviceCollection.AddSingleton<IExceptionHandler, ExceptionHandler>();
        serviceCollection.AddSingleton<IFileService, FileService>();
        serviceCollection.AddSingleton<IHttpService, HttpService>();
        serviceCollection.AddSingleton<ILanguageService, LanguageService>();
        serviceCollection.AddSingleton<ILoggingService, LoggingService>();
        serviceCollection.AddSingleton<IPlaylistCreator, PlaylistCreator>();
        serviceCollection.AddSingleton<IResilienceService, ResilienceService>();
        serviceCollection.AddSingleton<ISettingsService, SettingsService>();
        serviceCollection.AddSingleton<ITagService, TagService>();
        serviceCollection.AddSingleton<IThemeService, ThemeService>();
        serviceCollection.AddSingleton<ITrackFileService, TrackFileService>();
        serviceCollection.AddSingleton<IUpdatesService, UpdatesService>();
        serviceCollection.AddSingleton<WindowMain>();

        return serviceCollection;
    }
}
