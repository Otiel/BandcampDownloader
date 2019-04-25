using System;
using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class JsonTrack {

        [JsonProperty("duration")]
        public Double Duration { get; set; }

        [JsonProperty("file")]
        public JsonMp3File File { get; set; }

        [JsonProperty("lyrics")]
        public String Lyrics { get; set; }

        [JsonProperty("track_num")]
        public int Number { get; set; }

        [JsonProperty("title")]
        public String Title { get; set; }

        public Track ToTrack(Album album) {
            return new Track(album) {
                Duration = Duration,
                Mp3Url = (File.Url.StartsWith("//") ? "http:" : "") + File.Url, // "//example.com" Uri lacks protocol
                Number = Number == 0 ? 1 : Number, // For bandcamp track pages, Number will be 0. Set 1 instead
                Title = Title,
                Lyrics = Lyrics
            };
        }
    }
}