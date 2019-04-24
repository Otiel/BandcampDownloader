using System;
using System.ComponentModel;
using Config.Net;

namespace BandcampDownloader {

    public enum Language {
        [Description("English")]
        en,
        [Description("Français (French)")]
        fr,
        [Description("Deutsch (German)")]
        de
    }

    public enum PlaylistFormat {
        [Description("M3U")]
        m3u,
        [Description("PLS")]
        pls,
        [Description("WPL (Windows Media Player)")]
        wpl,
        [Description("ZPL (Zune Media Player)")]
        zpl
    }

    public enum ProxyType {
        None,
        System,
        Manual,
    }

    public enum TagEditAction {
        [Description("Empty tag")]
        Empty,
        [Description("Save in tag")]
        Modify,
        [Description("Do not modify")]
        DoNotModify
    }

    public enum TagRemoveAction {
        [Description("Empty tag")]
        Empty,
        [Description("Do not modify")]
        DoNotModify
    }

    public interface IUserSettings {

        [Option(DefaultValue = 0.05)]
        Double AllowedFileSizeDifference { get; set; }

        [Option(DefaultValue = true)]
        Boolean CheckForUpdates { get; set; }

        [Option(DefaultValue = "{album}")]
        String CoverArtFileNameFormat { get; set; }

        [Option(DefaultValue = true)]
        Boolean CoverArtInFolderConvertToJpg { get; set; }

        [Option(DefaultValue = 1000)]
        int CoverArtInFolderMaxSize { get; set; }

        [Option(DefaultValue = false)]
        Boolean CoverArtInFolderResize { get; set; }

        [Option(DefaultValue = true)]
        Boolean CoverArtInTagsConvertToJpg { get; set; }

        [Option(DefaultValue = 1000)]
        int CoverArtInTagsMaxSize { get; set; }

        [Option(DefaultValue = true)]
        Boolean CoverArtInTagsResize { get; set; }

        [Option(DefaultValue = false)]
        Boolean CreatePlaylist { get; set; }

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

        [Option(DefaultValue = false)]
        Boolean EnableApplicationSounds { get; set; }

        [Option(DefaultValue = "{tracknum} {artist} - {title}.mp3")]
        String FileNameFormat { get; set; }

        [Option(DefaultValue = Language.en)]
        Language Language { get; set; }

        [Option(DefaultValue = true)]
        Boolean M3uExtended { get; set; }

        [Option(DefaultValue = true)]
        Boolean ModifyTags { get; set; }

        [Option(DefaultValue = "{album}")]
        String PlaylistFileNameFormat { get; set; }

        [Option(DefaultValue = PlaylistFormat.m3u)]
        PlaylistFormat PlaylistFormat { get; set; }

        [Option(DefaultValue = ProxyType.System)]
        ProxyType Proxy { get; set; }

        [Option(DefaultValue = "")]
        String ProxyHttpAddress { get; set; }

        [Option(DefaultValue = "")]
        int ProxyHttpPort { get; set; }

        [Option(DefaultValue = true)]
        Boolean RetrieveFilesSize { get; set; }

        [Option(DefaultValue = false)]
        Boolean SaveCoverArtInFolder { get; set; }

        [Option(DefaultValue = true)]
        Boolean SaveCoverArtInTags { get; set; }

        [Option(DefaultValue = false)]
        Boolean ShowVerboseLog { get; set; }

        [Option(DefaultValue = TagEditAction.Modify)]
        TagEditAction TagAlbumArtist { get; set; }

        [Option(DefaultValue = TagEditAction.Modify)]
        TagEditAction TagAlbumTitle { get; set; }

        [Option(DefaultValue = TagEditAction.Modify)]
        TagEditAction TagArtist { get; set; }

        [Option(DefaultValue = TagRemoveAction.Empty)]
        TagRemoveAction TagComments { get; set; }

        [Option(DefaultValue = TagEditAction.Modify)]
        TagEditAction TagLyrics { get; set; }

        [Option(DefaultValue = TagEditAction.Modify)]
        TagEditAction TagTrackNumber { get; set; }

        [Option(DefaultValue = TagEditAction.Modify)]
        TagEditAction TagTrackTitle { get; set; }

        [Option(DefaultValue = TagEditAction.Modify)]
        TagEditAction TagYear { get; set; }
    }
}