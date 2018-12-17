using System;
using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class JsonAlbumData {

        [JsonProperty("title")]
        public String AlbumTitle { get; set; }
    }
}