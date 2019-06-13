using System;
using System.Windows.Media;
using NLog;

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
                    color = App.UserSettings.Theme == Skin.Dark ? new SolidColorBrush((Color) ColorConverter.ConvertFromString("#f0f0f0")) : Brushes.Black;
                    break;
                case LogType.VerboseInfo:
                    color = App.UserSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue;
                    break;
                case LogType.IntermediateSuccess:
                    color = App.UserSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue;
                    break;
                case LogType.Success:
                    color = App.UserSettings.Theme == Skin.Dark ? Brushes.Lime : Brushes.Green;
                    break;
                case LogType.Warning:
                    color = App.UserSettings.Theme == Skin.Dark ? Brushes.Orange : Brushes.OrangeRed;
                    break;
                case LogType.Error:
                    color = App.UserSettings.Theme == Skin.Dark ? Brushes.Red : Brushes.Red;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return color;
        }

        /// <summary>
        /// Writes the specified Exception and all its InnerException to the application log file.
        /// </summary>
        /// <param name="exception">The Exception to log.</param>
        public static void LogExceptionAndInnerExceptionsToFile(Exception exception) {
            LogExceptionToFile(exception);

            if (exception.InnerException != null) {
                LogExceptionAndInnerExceptionsToFile(exception.InnerException);
            }
        }

        /// <summary>
        /// Writes the specified Exception to the application log file.
        /// </summary>
        /// <param name="exception">The Exception to log.</param>
        public static void LogExceptionToFile(Exception exception) {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Log(LogLevel.Fatal, String.Format("{0} {1}", exception.GetType().ToString(), exception.Message));
            logger.Log(LogLevel.Fatal, exception.StackTrace);
        }
    }
}