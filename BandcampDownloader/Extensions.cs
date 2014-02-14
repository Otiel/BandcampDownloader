using System;

namespace BandcampDownloader {

    internal static class Extensions {
        /// <summary>
        /// Replaces the forbidden chars \ / : * ? " &lt; &gt; | from the System.String
        /// object by an underscore _ in order to be used for a Windows file or folder.
        /// </summary>
        public static String ToAllowedFileName(this String fileName) {
            if (fileName == null) {
                throw new ArgumentNullException("fileName");
            }

            fileName = fileName.Replace("\\", "_");
            fileName = fileName.Replace("/", "_");
            fileName = fileName.Replace(":", "_");
            fileName = fileName.Replace("*", "_");
            fileName = fileName.Replace("?", "_");
            fileName = fileName.Replace("\"", "_");
            fileName = fileName.Replace("<", "_");
            fileName = fileName.Replace(">", "_");
            fileName = fileName.Replace("|", "_");
            fileName = fileName.Replace(Environment.NewLine, "_");

            return fileName;
        }
    }
}