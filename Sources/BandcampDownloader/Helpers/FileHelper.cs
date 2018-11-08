using System;
using System.Net;

namespace BandcampDownloader {

    internal static class FileHelper {

        /// <summary>
        /// Returns the size of the file located at the provided URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="protocolMethod">The protocol method to use in order to retrieve the file
        /// size.</param>
        /// <returns>The size of the file located at the provided URL.</returns>
        public static long GetFileSize(String url, String protocolMethod) {
            var webRequest = HttpWebRequest.Create(url);
            webRequest.Method = protocolMethod;
            long fileSize;
            try {
                using (WebResponse webResponse = webRequest.GetResponse()) {
                    fileSize = webResponse.ContentLength;
                }
            } catch (Exception e) {
                throw new Exception("Could not retrieve file size.", e);
            }
            return fileSize;
        }
    }
}