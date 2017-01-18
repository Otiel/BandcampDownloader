using System;

namespace BandcampDownloader {

    internal class Track {
        public String Title  { get; set; }
        public int    Number { get; set; }
        public String Mp3Url { get; set; }

        public String GetFileName(String artist)
        {
            String fileName =
                UserSettings.FilenameFormat.Replace("{artist}", artist)
                    .Replace("{title}", Title)
                    .Replace("{tracknum}", Number.ToString("00"));
            return fileName.ToAllowedFileName();
        }
    }
}