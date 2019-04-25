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
            WebRequest webRequest = HttpWebRequest.Create(url);
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

        /// <summary>
        /// Returns the download path for the specified album from the specified path format, by replacing the
        /// placeholders strings with their corresponding values. If the path is too long (&gt; 247 characters), it will
        /// be stripped.
        /// </summary>
        /// <param name="downloadPath">The download path to parse.</param>
        /// <param name="album">The album.</param>
        public static String ParseAlbumPath(String downloadPath, Album album) {
            downloadPath = downloadPath.Replace("{year}", album.ReleaseDate.Year.ToString().ToAllowedFileName());
            downloadPath = downloadPath.Replace("{month}", album.ReleaseDate.Month.ToString("00").ToAllowedFileName());
            downloadPath = downloadPath.Replace("{day}", album.ReleaseDate.Day.ToString("00").ToAllowedFileName());
            downloadPath = downloadPath.Replace("{artist}", album.Artist.ToAllowedFileName());
            downloadPath = downloadPath.Replace("{album}", album.Title.ToAllowedFileName());

            if (downloadPath.Length >= 248) {
                // Windows doesn't do well with path >= 248 characters (and path + filename >= 260 characters)
                downloadPath = downloadPath.Substring(0, 247);
            }

            return downloadPath;
        }
    }
}