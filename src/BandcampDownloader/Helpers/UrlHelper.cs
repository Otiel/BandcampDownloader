using System.Diagnostics;

namespace BandcampDownloader.Helpers;

internal static class UrlHelper
{
    public static void OpenUrlInBrowser(string url)
    {
        using var p = new Process();
        p.StartInfo.FileName = url;
        p.StartInfo.UseShellExecute = true;
        p.Start();
    }
}
