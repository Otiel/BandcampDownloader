using System;
using System.Collections.Generic;

namespace BandcampDownloader {

    internal class Album {

        /// <summary>
        /// The album artist.
        /// </summary>
        public String Artist { get; set; }

        /// <summary>
        /// The URL where the artwork should be downloaded from.
        /// </summary>
        public String ArtworkUrl { get; set; }

        /// <summary>
        /// True if the album has an artwork; false otherwise.
        /// </summary>
        public Boolean HasArtwork {
            get {
                return ArtworkUrl != null;
            }
        }

        /// <summary>
        /// The release date of the album.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// The album title.
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// The list of tracks contained in the album.
        /// </summary>
        public List<Track> Tracks { get; set; }
        /// <summary>
        /// Sets the Path property of each Track from the specified folder.
        /// </summary>
        /// <param name="folderPath">The full path where the tracks files should be saved.</param>
        public void SetTracksPath(String folderPath) {
            foreach (Track track in Tracks) {
                track.SetPath(folderPath, this);
            }
        }
    }
}