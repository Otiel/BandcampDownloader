using System;
using System.IO;
using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class UserSettings {
        public Boolean ConvertCoverArtToJpg { get; set; }
        public String CoverArtMaxSize { get; set; }
        public Boolean DownloadOneAlbumAtATime { get; set; }
        public String DownloadsLocation { get; set; }
        public Boolean ForceDownloadsOfAllAlbums { get; set; }
        public Boolean ResizeCoverArt { get; set; }
        public Boolean SaveCoverArtInFolder { get; set; }
        public Boolean SaveCoverArtInTags { get; set; }
        public Boolean ShowVerboseLog { get; set; }
        public Boolean TagTracks { get; set; }
        // Annotation required to allow serialization of static field
        [JsonProperty]
        public static int DownloadMaxTries { get; set; }
        // Time in seconds between retries
        [JsonProperty]
        public static double DownloadRetryCooldown { get; set; }
        // Exponential per cooldown - ex. (value of 1.2 would yield cooldowns of x^(1.2^0), x^(1.2^1), x^(1.2^2), ..)
        [JsonProperty]
        public static double DownloadRetryExponential { get; set; }
        [JsonProperty]
        public static double AllowableFileSizeDifference { get; set; }

        /// <summary>
        ///  Creates a new UserSettings with default values.
        /// </summary>
        public UserSettings() {
            ResetToDefault();
        }

        /// <summary>
        /// Reads the JSON content from the specified file and returns the equivalent UserSettings.
        /// </summary>
        /// <param name="filePath">The absolute path to the file.</param>
        public static UserSettings ReadFromFile(String filePath) {
            var userSettings = new UserSettings();

            String jsonStr = File.ReadAllText(filePath);

            // Deserialize JSON
            userSettings = JsonConvert.DeserializeObject<UserSettings>(jsonStr);

            return userSettings;
        }

        /// <summary>
        /// Resets all values to default.
        /// </summary>
        private void ResetToDefault() {
            ConvertCoverArtToJpg = true;
            CoverArtMaxSize = "1000";
            DownloadOneAlbumAtATime = false;
            DownloadsLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
            ForceDownloadsOfAllAlbums = false;
            ResizeCoverArt = true;
            SaveCoverArtInFolder = false;
            SaveCoverArtInTags = true;
            ShowVerboseLog = false;
            TagTracks = true;
            DownloadMaxTries = 10;
            DownloadRetryCooldown = 0;
            DownloadRetryExponential = 1;
            AllowableFileSizeDifference = 0.05;
        }

        /// <summary>
        /// Saves the current UserSettings to the specified file in JSON format.
        /// </summary>
        /// <param name="filePath">The absolute path to the file.</param>
        public void SaveToFile(String filePath) {
            // Create directory if it does not exist
            String fileDirectory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(fileDirectory)) {
                Directory.CreateDirectory(fileDirectory);
            }

            // Serialize user settings
            String jsonStr = JsonConvert.SerializeObject(this, Formatting.Indented);

            // Write to file
            File.WriteAllText(filePath, jsonStr);
        }
    }
}