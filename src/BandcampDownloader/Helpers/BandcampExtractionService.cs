using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using BandcampDownloader.Model;
using BandcampDownloader.Model.JSON;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NLog;

namespace BandcampDownloader.Helpers;

internal interface IBandcampExtractionService
{
    /// <summary>
    /// Retrieves the data on the album of the specified Bandcamp page.
    /// </summary>
    /// <param name="htmlCode">The HTML source code of a Bandcamp album page.</param>
    /// <returns>The data on the album of the specified Bandcamp page.</returns>
    Album GetAlbum(string htmlCode);

    /// <summary>
    /// Retrieves all the albums URL existing on the specified Bandcamp page.
    /// </summary>
    /// <param name="htmlCode">The HTML source code of a Bandcamp page.</param>
    /// <param name="artistPage">The URL to the artist page.</param>
    /// <returns>The albums URL existing on the specified Bandcamp page.</returns>
    List<string> GetAlbumsUrl(string htmlCode, string artistPage);
}

internal sealed class BandcampExtractionService : IBandcampExtractionService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public Album GetAlbum(string htmlCode)
    {
        // Keep the interesting part of htmlCode only
        if (!TryGetAlbumData(htmlCode, out var htmlAlbumData))
        {
            throw new Exception("Could not retrieve album data in HTML code.");
        }

        // Fix some wrongly formatted JSON in source code
        htmlAlbumData = FixJson(htmlAlbumData);

        // Deserialize JSON
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };
        var album = JsonConvert.DeserializeObject<JsonAlbum>(htmlAlbumData, settings).ToAlbum();

        // Extract lyrics from album page
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlCode);
        foreach (var track in album.Tracks)
        {
            var lyricsElement = htmlDoc.GetElementbyId("lyrics_row_" + track.Number);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse : lyricsElement can be null
            if (lyricsElement != null)
            {
                track.Lyrics = lyricsElement.InnerText.Trim();
            }
        }

        return album;
    }

    public List<string> GetAlbumsUrl(string htmlCode, string artistPage)
    {
        // Get albums ("real" albums or track-only pages) relative urls
        var regex = new Regex("href=\"(?<url>/(album|track)/.*)\"");
        if (!regex.IsMatch(htmlCode))
        {
            throw new NoAlbumFoundException();
        }

        var albumsUrl = new List<string>();
        foreach (Match m in regex.Matches(htmlCode))
        {
            albumsUrl.Add(artistPage + m.Groups["url"].Value);
        }

        // Remove duplicates
        albumsUrl = albumsUrl.Distinct().ToList();
        return albumsUrl;
    }

    private static string FixJson(string albumData)
    {
        // Some JSON is not correctly formatted in bandcamp pages, so it needs to be fixed before we can deserialize it

        // In trackinfo property, we have for instance:
        // url: "http://verbalclick.bandcamp.com" + "/album/404"
        // -> Remove the " + "
        var regex = new Regex("(?<root>url: \".+)\" \\+ \"(?<album>.+\",)");
        var fixedData = regex.Replace(albumData, "${root}${album}");

        return fixedData;
    }

    private static bool TryGetAlbumData(string htmlCode, out string albumData)
    {
        albumData = null;

        const string startString = "data-tralbum=\"{";
        const string stopString = "}\"";

        if (!htmlCode.Contains(startString))
        {
            _logger.Warn($"Could not find {nameof(startString)} in {nameof(htmlCode)}");
            return false;
        }

        var startIndex = htmlCode.IndexOf(startString, StringComparison.Ordinal) + startString.Length - 1;
        var albumDataTemp = htmlCode[startIndex..];

        var length = albumDataTemp.IndexOf(stopString, StringComparison.Ordinal) + 1;
        albumDataTemp = albumDataTemp[..length];

        albumData = WebUtility.HtmlDecode(albumDataTemp);

        return true;
    }
}
