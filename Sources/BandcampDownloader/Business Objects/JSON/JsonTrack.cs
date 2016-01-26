using System;
using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class JsonTrack {
        [JsonProperty("file")]
        public JsonMp3File File { get; set; }

        [JsonProperty("title")]
        public String Title { get; set; }

        [JsonProperty("track_num")]
        public int Number { get; set; }

        public Track ToTrack() {
            return new Track() {
                Mp3Url = (File.Url.StartsWith("//") ? "http:" : "") + File.Url,// "//example.com" Uri lacks protocol :(
                Number = Number,
                Title = Title
            };
        }
    }
}
