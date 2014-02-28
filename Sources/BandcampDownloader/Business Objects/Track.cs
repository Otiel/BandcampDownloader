using System;

namespace BandcampDownloader {

    internal class Track {
        public String Title  { get; set; }
        public int    Number { get; set; }
        public String Mp3Url { get; set; }

        public String GetFileName(String artist) {
            String fileName = Number.ToString("00") + " " + artist + " - " + Title + ".mp3";
            return fileName.ToAllowedFileName();
        }
    }
}