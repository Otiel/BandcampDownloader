using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BandcampDownloader {

    internal static class FileHelper {

        /// <summary>
        /// Asynchronously copies an existing file to a new file.
        /// </summary>
        /// <param name="sourceFile">The file to copy.</param>
        /// <param name="destinationFile">The name of the destination file.</param>
        public static async Task CopyFileAsync(string sourceFile, string destinationFile) {
            // https://stackoverflow.com/a/35467471/825024
            const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            const int bufferSize = 4096;

            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions)) {
                using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions)) {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }

        /// <summary>
        /// Returns the size of the file located at the provided URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="protocolMethod">The protocol method to use in order to retrieve the file size.</param>
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