using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BandcampDownloader {
    class Album {
        public String Artist { get; set; }
        public String ArtworkUrl { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<Track> Tracks { get; set; }
        public String Title { get; set; }
    }
}
