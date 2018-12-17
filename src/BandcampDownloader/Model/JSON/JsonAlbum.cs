using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BandcampDownloader {

    internal class JsonAlbum {
        private readonly String _urlEnd = "_0.jpg";
        private readonly String _urlStart = "https://f4.bcbits.com/img/a"; // Uses the art_id variable to retrieve the image from Bandcamp hosting site

        [JsonProperty("current")]
        public JsonAlbumData AlbumData { get; set; }

        [JsonProperty("art_id")]
        public String ArtId { get; set; }

        [JsonProperty("artist")]
        public String Artist { get; set; }

        [JsonProperty("album_release_date")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("trackinfo")]
        public List<JsonTrack> Tracks { get; set; }

        public Album ToAlbum() {
            return new Album() {
                Artist = Artist,
                // Some albums do not have a cover art
                ArtworkUrl = ArtId == null ? null : _urlStart + ArtId.PadLeft(10, '0') + _urlEnd,
                ReleaseDate = ReleaseDate,
                Title = AlbumData.AlbumTitle,
                // Some tracks do not have their URL filled on some albums (pre-release...)
                // Forget those tracks here
                Tracks = Tracks.Where(t => t.File != null).Select(t => t.ToTrack()).ToList()
            };
        }
    }
}