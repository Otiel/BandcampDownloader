using System;
using System.Windows.Media;

namespace BandcampDownloader {

    internal static class LogHelper {

        /// <summary>
        /// Returns the color associated to the specified log type.
        /// </summary>
        /// <param name="logType">The type of the log.</param>
        public static SolidColorBrush GetColor(LogType logType) {
            SolidColorBrush color;

            switch (logType) {
                case LogType.Info:
                    color = Brushes.Black;
                    break;
                case LogType.VerboseInfo:
                    color = Brushes.MediumBlue;
                    break;
                case LogType.IntermediateSuccess:
                    color = Brushes.MediumBlue;
                    break;
                case LogType.Success:
                    color = Brushes.Green;
                    break;
                case LogType.Warning:
                    color = Brushes.OrangeRed;
                    break;
                case LogType.Error:
                    color = Brushes.Red;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return color;
        }
    }
}