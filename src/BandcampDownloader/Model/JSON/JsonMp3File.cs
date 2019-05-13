using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class JsonMp3File {

        [JsonProperty("mp3-128")]
        public string Url { get; set; }
    }
}