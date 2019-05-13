using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NLog;

namespace BandcampDownloader {

    internal static class Extensions {

        /// <summary>
        /// Returns true if scroll position of the current RichTextBox is at the end; false otherwise.
        /// </summary>
        public static bool IsScrolledToEnd(this RichTextBox richTextBox) {
            return richTextBox.VerticalOffset > richTextBox.ExtentHeight - richTextBox.ViewportHeight - 10;
        }

        /// <summary>
        /// Replaces the forbidden chars \ / : * ? " &lt; &gt; | from the System.String
        /// object by an underscore _ in order to be used for a Windows file or folder.
        /// </summary>
        public static string ToAllowedFileName(this string fileName) {
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
            fileName = Regex.Replace(fileName, @"\s+", " ");

            return fileName;
        }

        /// <summary>
        /// Returns the NLog.LogLevel associated to the specified LogType.
        /// </summary>
        public static LogLevel ToNLogLevel(this LogType logType) {
            switch (logType) {
                case LogType.VerboseInfo:
                    return LogLevel.Debug;
                case LogType.Info:
                    return LogLevel.Info;
                case LogType.IntermediateSuccess:
                    return LogLevel.Info;
                case LogType.Success:
                    return LogLevel.Info;
                case LogType.Warning:
                    return LogLevel.Warn;
                case LogType.Error:
                    return LogLevel.Error;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}