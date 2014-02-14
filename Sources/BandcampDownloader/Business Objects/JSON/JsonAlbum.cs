using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace BandcampDownloader {

    internal class JsonAlbum {
        [JsonProperty("artist")]
        public String Artist { get; set; }

        [JsonProperty("artFullsizeUrl")]
        public String ArtWorkUrl { get; set; }

        [JsonProperty("album_release_date")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("trackinfo")]
        public List<JsonTrack> Tracks { get; set; }

        [JsonProperty("current")]
        public JsonAlbumData AlbumData { get; set; }

        public Album ToAlbum() {
            return new Album() {
                Artist = Artist,
                ArtworkUrl = ArtWorkUrl,
                ReleaseDate = ReleaseDate,
                Title = AlbumData.AlbumTitle,
                // Some tracks do not have their URL filled on some albums (pre-release...)
                // Forget those tracks here
                Tracks = Tracks.Where(t => t.File != null).Select(t => t.ToTrack()).ToList()
            };
        }
    }
}