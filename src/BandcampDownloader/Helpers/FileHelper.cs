using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandcampDownloader
{
    internal static class FileHelper
    {
        /// <summary>
        /// Asynchronously copies an existing file to a new file.
        /// </summary>
        /// <param name="sourceFile">The file to copy.</param>
        /// <param name="destinationFile">The name of the destination file.</param>
        public static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            // https://stackoverflow.com/a/35467471/825024
            const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            const int bufferSize = 4096;

            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions))
            {
                using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions))
                {
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
        public static async Task<long> GetFileSizeAsync(string url, string protocolMethod)
        {
            var webRequest = HttpWebRequest.Create(url);
            webRequest.Method = protocolMethod;
            long fileSize;
            try
            {
                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    fileSize = webResponse.ContentLength;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not retrieve file size.", e);
            }
            return fileSize;
        }

        /// <summary>
        /// Replaces the forbidden characters \ / : * ? " &lt; &gt; | from the System.String object by an underscore _ in
        /// order to be used for a Windows file or folder.
        /// </summary>
        public static string ToAllowedFileName(this string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            // Rules are defined here: https://docs.microsoft.com/en-us/windows/desktop/FileIO/naming-a-file

            // Replace reserved characters by '_'
            fileName = fileName.Replace("\\", "_");
            fileName = fileName.Replace("/", "_");
            fileName = fileName.Replace(":", "_");
            fileName = fileName.Replace("*", "_");
            fileName = fileName.Replace("?", "_");
            fileName = fileName.Replace("\"", "_");
            fileName = fileName.Replace("<", "_");
            fileName = fileName.Replace(">", "_");
            fileName = fileName.Replace("|", "_");

            // Replace newline by '_'
            fileName = fileName.Replace(Environment.NewLine, "_");

            // Remove trailing dot(s)
            fileName = Regex.Replace(fileName, @"\.+$", "");

            // Replace whitespace(s) by ' '
            fileName = Regex.Replace(fileName, @"\s+", " ");

            // Remove trailing whitespace(s) /!\ Must be last
            fileName = Regex.Replace(fileName, @"\s+$", "");

            return fileName;
        }
    }
}