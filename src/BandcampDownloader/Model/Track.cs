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
        /// The local path (full path with file name) where the track file should be saved.
        /// </summary>
        public String Path { get; private set; }

        /// <summary>
        /// The track title.
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// Sets the Path property of the current Track from the specified folder.
        /// </summary>
        /// <param name="folderPath">The full path where the track file should be saved.</param>
        /// <param name="album">The album of the current Track.</param>
        public void SetPath(String folderPath, Album album) {
            String fileName = ParseTrackFileName(album);

            Path = folderPath + "\\" + fileName;
            if (Path.Length >= 260) {
                // Windows doesn't do well with path + filename >= 260 characters (and path >= 248 characters)
                // Path has been shorten to 247 characters before, so we have 12 characters max left for filename.ext
                int fileNameMaxLength = 12 - System.IO.Path.GetExtension(Path).Length;
                Path = folderPath + "\\" + fileName.Substring(0, fileNameMaxLength) + System.IO.Path.GetExtension(Path);
            }
        }

        /// <summary>
        /// Returns the file name to be used for the specified track and album from the file name format saved in the
        /// UserSettings, by replacing the placeholders strings with their corresponding values.
        /// The returned file name DOES contain the extension.
        /// </summary>
        /// <param name="album">The album of the specified track.</param>
        private String ParseTrackFileName(Album album) {
            String fileName = App.UserSettings.FileNameFormat
                .Replace("{year}", album.ReleaseDate.Year.ToString())
                .Replace("{month}", album.ReleaseDate.Month.ToString("00"))
                .Replace("{day}", album.ReleaseDate.Day.ToString("00"))
                .Replace("{album}", album.Title)
                .Replace("{artist}", album.Artist)
                .Replace("{title}", Title)
                .Replace("{tracknum}", Number.ToString("00"));
            return fileName.ToAllowedFileName();
        }
    }
}