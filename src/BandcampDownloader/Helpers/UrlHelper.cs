using System.Diagnostics;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Settings;

namespace BandcampDownloader.Helpers;

internal static class UrlHelper
{
    public static string GetHttpUrlIfNeeded(string url)
    {
        var userSettings = DependencyInjectionHelper.GetService<ISettingsService>().GetUserSettings();
        return userSettings.UseHttpInsteadOfHttps ? url.Replace("https", "http") : url;
    }

    public static void OpenUrlInBrowser(string url)
    {
        using var p = new Process();
        p.StartInfo.FileName = url;
        p.StartInfo.UseShellExecute = true;
        p.Start();
    }
}
