namespace BandcampDownloader.DependencyInjection;

internal static class DependencyInjectionHelper
{
    private static Container _container;

    public static IContainer InitializeContainer()
    {
        _container = new Container();
        return _container;
    }

    /// <summary>
    /// DO NOT USE unless dependency injection is not possible.
    /// </summary>
    public static T GetService<T>()
    {
        return _container.GetService<T>();
    }
}
