using BandcampDownloader.Core;
using BandcampDownloader.Helpers;
using BandcampDownloader.IO;
using BandcampDownloader.Localization;
using BandcampDownloader.Logging;
using BandcampDownloader.Net;
using BandcampDownloader.Settings;
using BandcampDownloader.Themes;
using BandcampDownloader.UI.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace BandcampDownloader.DependencyInjection;

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

        serviceCollection.AddSingleton<IBandcampHelper, BandcampHelper>();
        serviceCollection.AddSingleton<IDownloadManager, DownloadManager>();
        serviceCollection.AddSingleton<IExceptionHandler, ExceptionHandler>();
        serviceCollection.AddSingleton<IFileService, FileService>();
        serviceCollection.AddSingleton<IHttpService, HttpService>();
        serviceCollection.AddSingleton<ILanguageService, LanguageService>();
        serviceCollection.AddSingleton<ILoggingService, LoggingService>();
        serviceCollection.AddSingleton<IPlaylistCreator, PlaylistCreator>();
        serviceCollection.AddSingleton<ISettingsService, SettingsService>();
        serviceCollection.AddSingleton<IThemeService, ThemeService>();
        serviceCollection.AddSingleton<WindowMain>();

        return serviceCollection;
    }
}
