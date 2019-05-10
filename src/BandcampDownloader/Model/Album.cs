using System;
using System.Collections.Generic;

namespace BandcampDownloader {

    internal class Album {

        /// <summary>
        /// The album artist.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// The local path (full path with file name) where the artwork file should be saved.
        /// </summary>
        public string ArtworkPath { get; private set; }

        /// <summary>
        /// The local path (full path with file name) to the %TEMP% folder where the artwork file should be saved.
        /// </summary>
        public string ArtworkTempPath { get; private set; }

        /// <summary>
        /// The URL where the artwork should be downloaded from.
        /// </summary>
        public string ArtworkUrl { get; set; }

        /// <summary>
        /// True if the album has an artwork; false otherwise.
        /// </summary>
        public bool HasArtwork {
            get {
                return ArtworkUrl != null;
            }
        }

        /// <summary>
        /// The local path (full path) to the folder where the album should be saved.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The local path (full path with file name) where the playlist file should be saved.
        /// </summary>
        public string PlaylistPath { get; private set; }

        /// <summary>
        /// The release date of the album.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// The album title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The list of tracks contained in the album.
        /// </summary>
        public List<Track> Tracks { get; set; }

        /// <summary>
        /// Initializes a new Album.
        /// </summary>
        public Album(string artist, string artworkUrl, DateTime releaseDate, string title) {
            Artist = artist;
            ArtworkUrl = artworkUrl;
            ReleaseDate = releaseDate;
            Title = title;
            // Must be done after other properties are filled!
            Path = ParseFolderPath();
            // Must be done after Path is set!
            PlaylistPath = ParsePlaylistPath();
            SetArtworkPaths();
        }

        /// <summary>
        /// Returns the file extension to be used for the playlist, depending of the type of playlist defined in UserSettings.
        /// </summary>
        private static string GetPlaylistFileExtension() {
            switch (App.UserSettings.PlaylistFormat) {
                case PlaylistFormat.m3u:
                    return ".m3u";
                case PlaylistFormat.pls:
                    return ".pls";
                case PlaylistFormat.wpl:
                    return ".wpl";
                case PlaylistFormat.zpl:
                    return ".zpl";
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the file name to be used for the cover art of the specified album from the file name format saved in
        /// the UserSettings, by replacing the placeholders strings with their corresponding values.
        /// The returned file name does NOT contain the extension.
        /// </summary>
        private string ParseCoverArtFileName() {
            string fileName = App.UserSettings.CoverArtFileNameFormat
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
        private string ParseFolderPath() {
            string path = App.UserSettings.DownloadsPath;
            path = path.Replace("{year}", ReleaseDate.Year.ToString().ToAllowedFileName());
            path = path.Replace("{month}", ReleaseDate.Month.ToString("00").ToAllowedFileName());
            path = path.Replace("{day}", ReleaseDate.Day.ToString("00").ToAllowedFileName());
            path = path.Replace("{artist}", Artist.ToAllowedFileName());
            path = path.Replace("{album}", Title.ToAllowedFileName());

            if (path.Length >= 248) {
                // Windows doesn't do well with path >= 248 characters (and path + filename >= 260 characters)
                path = path.Substring(0, 247);
            }

            return path;
        }

        /// <summary>
        /// Returns the file name to be used for the playlist file of the specified album from the file name format saved
        /// in the UserSettings, by replacing the placeholders strings with their corresponding values.
        /// </summary>
        private string ParsePlaylistFileName() {
            string fileName = App.UserSettings.PlaylistFileNameFormat
                .Replace("{year}", ReleaseDate.Year.ToString())
                .Replace("{month}", ReleaseDate.Month.ToString("00"))
                .Replace("{day}", ReleaseDate.Day.ToString("00"))
                .Replace("{album}", Title)
                .Replace("{artist}", Artist);
            return fileName.ToAllowedFileName();
        }

        /// <summary>
        /// Returns the path to be used for the playlist file from the file name format saved in the UserSettings, by
        /// replacing the placeholders strings with their corresponding values. If the path is too long (&gt; 259
        /// characters), it will be stripped.
        /// </summary>
        private string ParsePlaylistPath() {
            string fileExt = GetPlaylistFileExtension();

            // Compute paths where to save artwork
            string filePath = Path + "\\" + ParsePlaylistFileName() + fileExt;

            if (filePath.Length >= 260) {
                // Windows doesn't do well with path + filename >= 260 characters (and path >= 248 characters)
                // Path has been shorten to 247 characters before, so we have 12 characters max left for "\filename.ext", so 11 character max for "filename.ext"
                int fileNameMaxLength = 11 - fileExt.Length;
                filePath = Path + "\\" + ParsePlaylistFileName().Substring(0, fileNameMaxLength) + fileExt;
            }

            return filePath;
        }

        /// <summary>
        /// Sets the ArtworkPath and ArtworkTempPath properties.
        /// </summary>
        private void SetArtworkPaths() {
            if (HasArtwork) {
                string artworkFileExt = System.IO.Path.GetExtension(ArtworkUrl);

                // In order to prevent #54 (artworkTempPath used at the same time by another downloading thread), we'll add a random number to the name of the artwork file saved in Temp directory
                string randomNumber = App.Random.Next(1, 1000).ToString("00#");

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