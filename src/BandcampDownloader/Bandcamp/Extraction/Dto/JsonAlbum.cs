using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using BandcampDownloader.Model;

namespace BandcampDownloader.Bandcamp.Extraction.Dto;

internal sealed class JsonAlbum
{
    private const string URL_END = "_0.jpg";
    private const string URL_START = "https://f4.bcbits.com/img/a"; // Uses the art_id variable to retrieve the image from Bandcamp hosting site

    [JsonPropertyName("current")]
    public JsonAlbumData AlbumData { get; init; }

    [JsonPropertyName("art_id")]
    public long? ArtId { get; init; }

    [JsonPropertyName("artist")]
    public string Artist { get; init; }

    [JsonPropertyName("album_release_date")]
    public DateTime? ReleaseDate { get; set; }

    [JsonPropertyName("trackinfo")]
    public List<JsonTrack> Tracks { get; init; }

    public Album ToAlbum()
    {
        // Some albums do not have a cover art
        var artworkUrl = ArtId == null ? null : URL_START + ArtId.ToString().PadLeft(10, '0') + URL_END;

        // Singles might not have a release date  #144
        var releaseDate = ReleaseDate ?? AlbumData.ReleaseDate;

        var album = new Album(Artist, artworkUrl, releaseDate, AlbumData.AlbumTitle);

        // Some tracks do not have their URL filled on some albums (pre-release...)
        // Forget those tracks here
        album.Tracks = Tracks.Where(t => t.File != null).Select(t => t.ToTrack(album)).ToList();

        return album;
    }
}
