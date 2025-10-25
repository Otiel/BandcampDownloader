using BandcampDownloader.Helpers;
using BandcampDownloader.Localization;
using BandcampDownloader.Logging;
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

        serviceCollection.AddSingleton<IExceptionHandler, ExceptionHandler>();
        serviceCollection.AddSingleton<ILanguageService, LanguageService>();
        serviceCollection.AddSingleton<ILoggingService, LoggingService>();
        serviceCollection.AddSingleton<IThemeService, ThemeService>();
        serviceCollection.AddSingleton<ISettingsService, SettingsService>();
        serviceCollection.AddSingleton<WindowMain>();

        return serviceCollection;
    }
}
