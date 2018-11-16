using System;
using Config.Net;

namespace BandcampDownloader {

    public interface IUserSettings {

        [Option(DefaultValue = 0.05)]
        Double AllowedFileSizeDifference { get; set; }

        [Option(DefaultValue = true)]
        Boolean AutoScrollLog { get; set; }

        [Option(DefaultValue = true)]
        Boolean ConvertCoverArtToJpg { get; set; }

        [Option(DefaultValue = 1000)]
        int CoverArtMaxSize { get; set; }

        [Option(DefaultValue = false)]
        Boolean DownloadArtistDiscography { get; set; }

        [Option(DefaultValue = 7)]
        int DownloadMaxTries { get; set; }

        [Option(DefaultValue = false)]
        Boolean DownloadOneAlbumAtATime { get; set; }

        // Time in seconds between retries
        [Option(DefaultValue = 0.2)]
        Double DownloadRetryCooldown { get; set; }

        // Exponential per cooldown - ex. (value of 1.2 would yield cooldowns of x^(1.2^0), x^(1.2^1), x^(1.2^2), ..)
        [Option(DefaultValue = 4.0)]
        Double DownloadRetryExponent { get; set; }

        [Option(DefaultValue = "")]
        String DownloadsPath { get; set; }

        [Option(DefaultValue = "{tracknum} {artist} - {title}.mp3")]
        String FileNameFormat { get; set; }

        [Option(DefaultValue = true)]
        Boolean ResizeCoverArt { get; set; }

        [Option(DefaultValue = true)]
        Boolean RetrieveFilesSize { get; set; }

        [Option(DefaultValue = false)]
        Boolean SaveCoverArtInFolder { get; set; }

        [Option(DefaultValue = true)]
        Boolean SaveCoverArtInTags { get; set; }

        [Option(DefaultValue = false)]
        Boolean ShowVerboseLog { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagRemoveComments { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagSaveAlbum { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagSaveAlbumArtist { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagSaveArtist { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagSaveLyrics { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagSaveTitle { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagSaveTrackNumber { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagSaveYear { get; set; }

        [Option(DefaultValue = true)]
        Boolean TagTracks { get; set; }
    }
}