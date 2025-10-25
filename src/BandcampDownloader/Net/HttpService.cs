using System;
using System.Net;
using System.Threading.Tasks;
using BandcampDownloader.Helpers;

namespace BandcampDownloader.Net;

internal interface IHttpService
{
    /// <summary>
    /// Returns the size of the file located at the provided URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="protocolMethod">The protocol method to use in order to retrieve the file size.</param>
    /// <returns>The size of the file located at the provided URL.</returns>
    Task<long> GetFileSizeAsync(string url, string protocolMethod);
}

internal sealed class HttpService : IHttpService
{
    public async Task<long> GetFileSizeAsync(string url, string protocolMethod)
    {
#pragma warning disable SYSLIB0014
        var webRequest = WebRequest.Create(UrlHelper.GetHttpUrlIfNeeded(url));
#pragma warning restore SYSLIB0014
        webRequest.Method = protocolMethod;

        long fileSize;
        try
        {
            using var webResponse = await webRequest.GetResponseAsync();
            fileSize = webResponse.ContentLength;
        }
        catch (Exception e)
        {
            throw new Exception("Could not retrieve file size.", e);
        }

        return fileSize;
    }
}
