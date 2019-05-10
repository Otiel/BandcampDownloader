using System;
using System.Net;
using System.Threading.Tasks;

namespace BandcampDownloader {

    internal static class FileHelper {

        /// <summary>
        /// Returns the size of the file located at the provided URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="protocolMethod">The protocol method to use in order to retrieve the file
        /// size.</param>
        /// <returns>The size of the file located at the provided URL.</returns>
        public static async Task<long> GetFileSizeAsync(string url, string protocolMethod) {
            WebRequest webRequest = HttpWebRequest.Create(url);
            webRequest.Method = protocolMethod;
            long fileSize;
            try {
                using (WebResponse webResponse = await webRequest.GetResponseAsync()) {
                    fileSize = webResponse.ContentLength;
                }
            } catch (Exception e) {
                throw new Exception("Could not retrieve file size.", e);
            }
            return fileSize;
        }
    }
}