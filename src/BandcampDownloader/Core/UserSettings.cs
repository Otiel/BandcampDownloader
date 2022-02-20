using System.ComponentModel;
using Config.Net;

namespace BandcampDownloader
{
    // List of languages with ISO language name, native name and codes https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
    public enum Language
    {
        [Description("English")]
        en,
        //[Description("Arabic (العربية)")]
        //ar,
        [Description("Chinese (Simplified) (汉语)")]
        zh,
        [Description("Croatian (hrvatski jezik)")]
        hr,
        [Description("Dutch (Nederlands)")]
        nl,
        //[Description("Esperanto")]
        //eo,
        [Description("French (Français)")]
        fr,
        [Description("German (Deutsch)")]
        de,
        [Description("Indonesian (Bahasa Indonesia)")]
        id,
        [Description("Italian (Italiano)")]
        it,
        //[Description("Korean (한국어)")]
        //ko,
        [Description("Norwegian Bokmål (Norsk Bokmål)")]
        nb_NO,
        [Description("Polish (język polski)")]
        pl,
        [Description("Portuguese (Português)")]
        pt,
        [Description("Portuguese Brazil (Português Brasil)")]
        pt_BR,
        [Description("Russian (русский)")]
        ru,
        [Description("Spanish (Español)")]
        es,
        [Description("Turkish (Türkçe)")]
        tr,
        [Description("Ukrainian (Українська)")]
        uk,
    }

    public enum PlaylistFormat
    {
        [Description("M3U")]
        m3u,
        [Description("PLS")]
        pls,
        [Description("WPL (Windows Media Player)")]
        wpl,
        [Description("ZPL (Zune Media Player)")]
        zpl,
    }

    public enum ProxyType
    {
        None,
        System,
        Manual,
    }

    public enum Skin
    {
        [Description("Dark")]
        Dark,
        [Description("Light")]
        Light,
    }

    public enum TagEditAction
    {
        [Description("Empty tag")]
        Empty,
        [Description("Save in tag")]
        Modify,
        [Description("Do not modify")]
        DoNotModify,
    }

    public enum TagRemoveAction
    {
        [Description("Empty tag")]
        Empty,
        [Description("Do not modify")]
        DoNotModify,
    }

    public interface IUserSettings
    {
        [Option(DefaultValue = 0.05)]
        double AllowedFileSizeDifference { get; set; }

        [Option(DefaultValue = true)]
        bool CheckForUpdates { get; set; }

        [Option(DefaultValue = "{album}")]
        string CoverArtFileNameFormat { get; set; }

        [Option(DefaultValue = true)]
        bool CoverArtInFolderConvertToJpg { get; set; }

        [Option(DefaultValue = 1000)]
        int CoverArtInFolderMaxSize { get; set; }

        [Option(DefaultValue = false)]
        bool CoverArtInFolderResize { get; set; }

        [Option(DefaultValue = true)]
        bool CoverArtInTagsConvertToJpg { get; set; }

        [Option(DefaultValue = 1000)]
        int CoverArtInTagsMaxSize { get; set; }

        [Option(DefaultValue = true)]
        bool CoverArtInTagsResize { get; set; }

        [Option(DefaultValue = false)]
        bool CreatePlaylist { get; set; }

        [Option(DefaultValue = false)]
        bool DownloadArtistDiscography { get; set; }

        [Option(DefaultValue = 7)]
        int DownloadMaxTries { get; set; }

        [Option(DefaultValue = false)]
        bool DownloadOneAlbumAtATime { get; set; }

        // Time in seconds between retries
        [Option(DefaultValue = 0.2)]
        double DownloadRetryCooldown { get; set; }

        // Exponential per cooldown - ex. (value of 1.2 would yield cooldowns of x^(1.2^0), x^(1.2^1), x^(1.2^2), ..)
        [Option(DefaultValue = 4.0)]
        double DownloadRetryExponent { get; set; }

        [Option(DefaultValue = "")]
        string DownloadsPath { get; set; }

        [Option(DefaultValue = false)]
        bool EnableApplicationSounds { get; set; }

        [Option(DefaultValue = "{tracknum} {artist} - {title}.mp3")]
        string FileNameFormat { get; set; }

        [Option(DefaultValue = Language.en)]
        Language Language { get; set; }

        [Option(DefaultValue = true)]
        bool M3uExtended { get; set; }

        [Option(DefaultValue = true)]
        bool ModifyTags { get; set; }

        [Option(DefaultValue = "{album}")]
        string PlaylistFileNameFormat { get; set; }

        [Option(DefaultValue = PlaylistFormat.m3u)]
        PlaylistFormat PlaylistFormat { get; set; }

        [Option(DefaultValue = ProxyType.System)]
        ProxyType Proxy { get; set; }

        [Option(DefaultValue = "")]
        string ProxyHttpAddress { get; set; }

        [Option(DefaultValue = "")]
        int ProxyHttpPort { get; set; }

        [Option(DefaultValue = true)]
        bool RetrieveFilesSize { get; set; }

        [Option(DefaultValue = false)]
        bool SaveCoverArtInFolder { get; set; }

        [Option(DefaultValue = true)]
        bool SaveCoverArtInTags { get; set; }

        [Option(DefaultValue = false)]
        bool ShowVerboseLog { get; set; }

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

        [Option(DefaultValue = Skin.Light)]
        Skin Theme { get; set; }
    }
}