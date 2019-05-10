using System;

namespace BandcampDownloader {

    internal class LogArgs: EventArgs {
        public LogType LogType { get; private set; }
        public string Message { get; private set; }

        public LogArgs(string message, LogType logType) {
            Message = message;
            LogType = logType;
        }
    }
}