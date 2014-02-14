using System;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace BandcampDownloader {

    internal static class BandcampHelper {

        public static Album GetAlbum(String htmlCode) {
            // Keep the interesting part of htmlCode only
            String albumData;
            try {
                albumData = GetAlbumData(htmlCode);
            } catch (Exception e) {
                throw new Exception("Could not retrieve album data in HTML code.", e);
            }

            // Fix some wrongly formatted JSON in source code
            albumData = FixJson(albumData);

            // Deserialize JSON
            Album album;
            try {
                album = JsonConvert.DeserializeObject<JsonAlbum>(albumData).ToAlbum();
            } catch (Exception e) {
                throw new Exception("Could not deserialize JSON data.", e);
            }

            return album;
        }

        private static String FixJson(String albumData) {
            // Some JSON is not correctly formatted in bandcamp pages, so it needs to be fixed
            // before we can deserialize it

            // In trackinfo property, we have for instance:
            //     url: "http://verbalclick.bandcamp.com" + "/album/404"
            // -> Remove the " + "
            var regex = new Regex("(?<root>url: \".+)\" \\+ \"(?<album>.+\",)");
            String fixedData = regex.Replace(albumData, "${root}${album}");

            return fixedData;
        }

        private static String GetAlbumData(String htmlCode) {
            String startString = "var TralbumData = {";
            String stopString = "};";

            if (htmlCode.IndexOf(startString) == -1) {
                // Could not find startString
                throw new Exception("Could not find the following string in HTML code: " +
                    "var TralbumData = {");
            }

            String albumDataTemp = htmlCode.Substring(htmlCode.IndexOf(startString) +
                startString.Length - 1);
            String albumData = albumDataTemp.Substring(0, albumDataTemp.IndexOf(stopString) + 1);

            return albumData;
        }
    }
}