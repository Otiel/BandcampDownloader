using System;
using System.Collections.Generic;
using System.Linq;
using BandcampDownloader.Model;
using Newtonsoft.Json;

namespace BandcampDownloader.Bandcamp.Extraction.Dto;

internal sealed class JsonAlbum
{
    private const string URL_END = "_0.jpg";
    private const string URL_START = "https://f4.bcbits.com/img/a"; // Uses the art_id variable to retrieve the image from Bandcamp hosting site

    [JsonProperty("current")]
    public JsonAlbumData AlbumData { get; set; }

    [JsonProperty("art_id")]
    public string ArtId { get; set; }

    [JsonProperty("artist")]
    public string Artist { get; set; }

    [JsonProperty("album_release_date")]
    public DateTime ReleaseDate { get; set; }

    [JsonProperty("trackinfo")]
    public List<JsonTrack> Tracks { get; set; }

    public Album ToAlbum()
    {
        // Some albums do not have a cover art
        var artworkUrl = ArtId == null ? null : URL_START + ArtId.PadLeft(10, '0') + URL_END;

        // Singles might not have a release date  #144
        if (ReleaseDate == new DateTime())
        {
            ReleaseDate = AlbumData.ReleaseDate;
        }

        var album = new Album(Artist, artworkUrl, ReleaseDate, AlbumData.AlbumTitle);

        // Some tracks do not have their URL filled on some albums (pre-release...)
        // Forget those tracks here
        album.Tracks = Tracks.Where(t => t.File != null).Select(t => t.ToTrack(album)).ToList();

        return album;
    }
}
