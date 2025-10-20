namespace BandcampDownloader;

public static class UrlHelper
{
    public static string GetHttpUrlIfNeeded(string url)
    {
        return App.UserSettings.UseHttpInsteadOfHttps ? url.Replace("https", "http") : url;
    }
}
