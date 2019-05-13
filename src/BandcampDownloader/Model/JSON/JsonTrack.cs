using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class JsonTrack {

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("file")]
        public JsonMp3File File { get; set; }

        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }

        [JsonProperty("track_num")]
        public int Number { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        public Track ToTrack(Album album) {
            string mp3Url = (File.Url.StartsWith("//") ? "http:" : "") + File.Url; // "//example.com" Uri lacks protocol
            int number = Number == 0 ? 1 : Number; // For bandcamp track pages, Number will be 0. Set 1 instead

            return new Track(album, Duration, Lyrics, mp3Url, number, Title);
        }
    }
}