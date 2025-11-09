using System;
using System.Threading.Tasks;
using System.Windows;

namespace BandcampDownloader.Threading;

internal static class ThreadUtils
{
    public static async Task ExecuteOnUiAsync(Action action)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            // Already on UI thread
            action();
        }
        else
        {
            await Application.Current.Dispatcher.InvokeAsync(action);
        }
    }

    public static async Task<T> ExecuteOnUiAsync<T>(Func<T> action)
    {
        T result;
        if (Application.Current.Dispatcher.CheckAccess())
        {
            // Already on UI thread
            result = action();
        }
        else
        {
            result = await Application.Current.Dispatcher.InvokeAsync(action);
        }

        return result;
    }
}
