namespace BandcampDownloader {

    internal class Track {

        /// <summary>
        /// The track album.
        /// </summary>
        public Album Album { get; set; }

        /// <summary>
        /// The track length (in seconds).
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// The track lyrics.
        /// </summary>
        public string Lyrics { get; set; }

        /// <summary>
        /// The URL where the track should be downloaded from.
        /// </summary>
        public string Mp3Url { get; set; }

        /// <summary>
        /// The track number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// The local path (full path with file name) where the track file should be saved.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The track title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Initializes a new Track.
        /// </summary>
        public Track(Album album, double duration, string lyrics, string mp3Url, int number, string title) {
            Album = album;
            Duration = duration;
            Lyrics = lyrics;
            Mp3Url = mp3Url;
            Number = number;
            Title = title;
            // Must be done after other properties are filled!
            Path = ParseTrackFilePath();
        }

        /// <summary>
        /// Returns the file name to be used for the track from the file name format saved in the UserSettings, by
        /// replacing the placeholders strings with their corresponding values. The returned file name DOES contain the extension.
        /// </summary>
        private string ParseTrackFileName() {
            string fileName = App.UserSettings.FileNameFormat
                .Replace("{year}", Album.ReleaseDate.Year.ToString())
                .Replace("{month}", Album.ReleaseDate.Month.ToString("00"))
                .Replace("{day}", Album.ReleaseDate.Day.ToString("00"))
                .Replace("{album}", Album.Title)
                .Replace("{artist}", Album.Artist)
                .Replace("{title}", Title)
                .Replace("{tracknum}", Number.ToString("00"));
            return fileName.ToAllowedFileName();
        }

        /// <summary>
        /// Returns the file path to be used for the track from the file name format saved in the UserSettings, by
        /// replacing the placeholders strings with their corresponding values. The returned file path DOES contain the extension.
        /// </summary>
        private string ParseTrackFilePath() {
            string fileName = ParseTrackFileName();

            string path = Album.Path + "\\" + fileName;
            if (path.Length >= 260) {
                // Windows doesn't do well with path + filename >= 260 characters (and path >= 248 characters)
                // album.Path has been shorten to 247 characters before, so we have 12 characters max left for "\filename.ext", so 11 character max for "filename.ext"
                int fileNameMaxLength = 11 - System.IO.Path.GetExtension(path).Length;
                path = Album.Path + "\\" + fileName.Substring(0, fileNameMaxLength) + System.IO.Path.GetExtension(path);
            }

            return path;
        }
    }
}