using System;
using System.Text.Json.Serialization;
using BandcampDownloader.Model;

namespace BandcampDownloader.Bandcamp.Extraction.Dto;

internal sealed class JsonTrack
{
    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("file")]
    public JsonMp3File File { get; set; }

    [JsonPropertyName("lyrics")]
    public string Lyrics { get; set; }

    [JsonPropertyName("track_num")]
    public int? Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    public Track ToTrack(Album album)
    {
        var mp3Url = (File.Url.StartsWith("//", StringComparison.Ordinal) ? "http:" : "") + File.Url; // "//example.com" Uri lacks protocol
        var number = Number ?? 1; // For bandcamp track pages, Number will be null. Set 1 instead

        return new Track(album, Duration, Lyrics, mp3Url, number, Title);
    }
}
