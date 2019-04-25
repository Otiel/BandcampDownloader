using System;

namespace BandcampDownloader {

    internal class Track {

        /// <summary>
        /// The track length (in seconds).
        /// </summary>
        public Double Duration { get; set; }

        /// <summary>
        /// The track lyrics.
        /// </summary>
        public String Lyrics { get; set; }

        /// <summary>
        /// The URL where the track should be downloaded from.
        /// </summary>
        public String Mp3Url { get; set; }

        /// <summary>
        /// The track number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// The track title.
        /// </summary>
        public String Title { get; set; }
    }
}