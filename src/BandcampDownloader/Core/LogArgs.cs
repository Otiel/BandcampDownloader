using System;

namespace BandcampDownloader {

    internal class LogArgs: EventArgs {
        public LogType LogType { get; private set; }
        public String Message { get; private set; }

        public LogArgs(String message, LogType logType) {
            Message = message;
            LogType = logType;
        }
    }
}