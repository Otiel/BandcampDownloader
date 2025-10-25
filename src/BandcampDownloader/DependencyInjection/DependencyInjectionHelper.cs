using System;
using BandcampDownloader.Helpers;
using BandcampDownloader.Logging;
using BandcampDownloader.Themes;
using BandcampDownloader.UI.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace BandcampDownloader.DependencyInjection;

internal static class DependencyInjectionHelper
{
    private static IServiceProvider _serviceProvider;

    public static IServiceProvider InitializeContainer()
    {
        var serviceCollection = GetServiceCollection();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        return _serviceProvider;
    }

    /// <summary>
    /// DO NOT USE unless dependency injection is not possible.
    /// </summary>
    public static T GetService<T>()
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IExceptionHandler, ExceptionHandler>();
        serviceCollection.AddSingleton<ILoggingService, LoggingService>();
        serviceCollection.AddSingleton<IThemeService, ThemeService>();
        serviceCollection.AddSingleton<WindowMain>();

        return serviceCollection;
    }
}
