using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class JsonAlbumData {

        [JsonProperty("title")]
        public string AlbumTitle { get; set; }
    }
}