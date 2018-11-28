using System;
using System.Collections.Generic;

namespace BandcampDownloader {

    internal class Album {
        public String Artist { get; set; }
        public String ArtworkUrl { get; set; }

        public Boolean HasArtwork {
            get {
                return ArtworkUrl != null;
            }
        }

        public DateTime ReleaseDate { get; set; }
        public String Title { get; set; }
        public List<Track> Tracks { get; set; }
    }
}