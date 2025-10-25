using System;
using BandcampDownloader.Logging;
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

    private static ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<WindowMain>();

        return serviceCollection;
    }
}
