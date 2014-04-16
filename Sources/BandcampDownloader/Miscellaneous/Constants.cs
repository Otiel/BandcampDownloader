using System;

namespace BandcampDownloader {

    internal static class Constants {
        public static readonly String UrlsHint = "Paste URLs of albums to download here.\nA Bandcamp URL looks like: http://[artist name].bandcamp.com/album/[album name]\nYou can specify multiple URLs by writing one URL per line.";

        public static readonly String ProjectWebsite = "https://github.com/Otiel/BandcampDownloader";

        public static readonly int DownloadMaxTries = 10;
    }
}