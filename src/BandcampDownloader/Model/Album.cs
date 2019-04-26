using System;
using System.Collections.Generic;

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
        /// The local path (full path) to the folder where the album should be saved.
        /// </summary>
        public String Path { get; private set; }

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
        /// Initializes a new Album.
        /// </summary>
        public Album(String artist, String artworkUrl, DateTime releaseDate, String title) {
            Artist = artist;
            ArtworkUrl = artworkUrl;
            ReleaseDate = releaseDate;
            Title = title;
            // Must be done after other properties are filled!
            Path = ParseFolderPath(App.UserSettings.DownloadsPath);
            SetArtworkPaths();
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

        /// <summary>
        /// Returns the folder path from the specified path format, by replacing the placeholders strings with their
        /// corresponding values. If the path is too long (&gt; 247 characters), it will be stripped.
        /// </summary>
        /// <param name="downloadPath">The download path to parse.</param>
        private String ParseFolderPath(String downloadPath) {
            downloadPath = downloadPath.Replace("{year}", ReleaseDate.Year.ToString().ToAllowedFileName());
            downloadPath = downloadPath.Replace("{month}", ReleaseDate.Month.ToString("00").ToAllowedFileName());
            downloadPath = downloadPath.Replace("{day}", ReleaseDate.Day.ToString("00").ToAllowedFileName());
            downloadPath = downloadPath.Replace("{artist}", Artist.ToAllowedFileName());
            downloadPath = downloadPath.Replace("{album}", Title.ToAllowedFileName());

            if (downloadPath.Length >= 248) {
                // Windows doesn't do well with path >= 248 characters (and path + filename >= 260 characters)
                downloadPath = downloadPath.Substring(0, 247);
            }

            return downloadPath;
        }

        /// <summary>
        /// Sets the ArtworkPath and ArtworkTempPath properties.
        /// </summary>
        private void SetArtworkPaths() {
            if (HasArtwork) {
                String artworkFileExt = System.IO.Path.GetExtension(ArtworkUrl);

                // In order to prevent #54 (artworkTempPath used at the same time by another downloading thread), we'll add a random number to the name of the artwork file saved in Temp directory
                String randomNumber = App.Random.Next(1, 1000).ToString("00#");

                // Compute paths where to save artwork
                ArtworkTempPath = System.IO.Path.GetTempPath() + "\\" + ParseCoverArtFileName() + randomNumber + artworkFileExt;
                ArtworkPath = Path + "\\" + ParseCoverArtFileName() + artworkFileExt;

                if (ArtworkTempPath.Length >= 260 || ArtworkPath.Length >= 260) {
                    // Windows doesn't do well with path + filename >= 260 characters (and path >= 248 characters)
                    // Path has been shorten to 247 characters before, so we have 12 characters max left for "\filename.ext", so 11 character max for "filename.ext"
                    // There may be only one path needed to shorten, but it's better to use the same file name in both places
                    int fileNameInTempMaxLength = 11 - randomNumber.Length - artworkFileExt.Length;
                    int fileNameInFolderMaxLength = 11 - artworkFileExt.Length;
                    ArtworkTempPath = System.IO.Path.GetTempPath() + "\\" + ParseCoverArtFileName().Substring(0, fileNameInTempMaxLength) + randomNumber + artworkFileExt;
                    ArtworkPath = Path + "\\" + ParseCoverArtFileName().Substring(0, fileNameInFolderMaxLength) + artworkFileExt;
                }
            }
        }
    }
}