using System;

namespace BandcampDownloader {

    internal class Track {
        public Double Duration { get; set; } // In seconds
        public String Lyrics { get; set; }
        public String Mp3Url { get; set; }
        public int Number { get; set; }
        public String Title { get; set; }
    }
}