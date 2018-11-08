using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BandcampDownloader {
    class JsonAlbumData {
        [JsonProperty("title")]
        public String AlbumTitle { get; set; }
    }
}
