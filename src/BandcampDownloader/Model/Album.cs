using System;
using System.Collections.Generic;
using System.IO;

namespace BandcampDownloader {

    internal class Album {

        /// <summary>
        /// The album artist.
        /// </summary>
        public String Artist { get; set; }

        /// <summary>
        /// The local path (full path with file name) where the artwork file should be saved.
        /// </summary>
        public String ArtworkPath { get; private set; }

        /// <summary>
        /// The local path (full path with file name) to the %TEMP% folder where the artwork file should be saved.
        /// </summary>
        public String ArtworkTempPath { get; private set; }

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
        /// Sets the ArtworkPath and ArtworkTempPath properties.
        /// </summary>
        /// <param name="folderPath">The full path to the folder where the artwork file should be saved.</param>
        public void SetArtworkPaths(String folderPath) {
            if (HasArtwork) {
                String artworkFileExt = Path.GetExtension(ArtworkUrl);

                // In order to prevent #54 (artworkTempPath used at the same time by another downloading thread), we'll add a random number to the name of the artwork file saved in Temp directory
                String randomNumber = App.Random.Next(1, 1000).ToString("00#");

                // Compute paths where to save artwork
                ArtworkTempPath = Path.GetTempPath() + "\\" + ParseCoverArtFileName() + randomNumber + artworkFileExt;
                ArtworkPath = folderPath + "\\" + ParseCoverArtFileName() + artworkFileExt;

                if (ArtworkTempPath.Length >= 260 || ArtworkPath.Length >= 260) {
                    // Windows doesn't do well with path + filename >= 260 characters (and path >= 248 characters)
                    // Path has been shorten to 247 characters before, so we have 12 characters max left for filename.ext
                    // There may be only one path needed to shorten, but it's better to use the same file name in both places
                    int fileNameInTempMaxLength = 12 - randomNumber.Length - artworkFileExt.Length;
                    int fileNameInFolderMaxLength = 12 - artworkFileExt.Length;
                    ArtworkTempPath = Path.GetTempPath() + "\\" + ParseCoverArtFileName().Substring(0, fileNameInTempMaxLength) + randomNumber + artworkFileExt;
                    ArtworkPath = folderPath + "\\" + ParseCoverArtFileName().Substring(0, fileNameInFolderMaxLength) + artworkFileExt;
                }
            }
        }

        /// <summary>
        /// Sets the Path property of each Track from the specified folder.
        /// </summary>
        /// <param name="folderPath">The full path to the folder where the tracks files should be saved.</param>
        public void SetTracksPath(String folderPath) {
            foreach (Track track in Tracks) {
                track.SetPath(folderPath, this);
            }
        }

        /// <summary>
        /// Returns the file name to be used for the cover art of the specified album from the file name format saved in
        /// the UserSettings, by replacing the placeholders strings with their corresponding values.
        /// The returned file name does NOT contain the extension.
        /// </summary>
        private String ParseCoverArtFileName() {
            String fileName = App.UserSettings.CoverArtFileNameFormat
                .Replace("{year}", ReleaseDate.Year.ToString())
                .Replace("{month}", ReleaseDate.Month.ToString("00"))
                .Replace("{day}", ReleaseDate.Day.ToString("00"))
                .Replace("{album}", Title)
                .Replace("{artist}", Artist);
            return fileName.ToAllowedFileName();
        }
    }
}