// ReSharper disable StringLiteralTypo

namespace BandcampDownloader.UnitTests.Resources;

internal static class ResourceAccessor
{
    public static FileInfo GetFileInfo(ResourceId resourceId)
    {
        var musicRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "Files", "Music");

        return resourceId switch
        {
            ResourceId.Music_Affektrecordings_Html => new FileInfo(Path.Combine(musicRoot, "affektrecordings.html")),
            ResourceId.Music_Afterdarkrecordings_Html => new FileInfo(Path.Combine(musicRoot, "afterdarkrecordings.html")),
            ResourceId.Music_Astropilotmusic_Html => new FileInfo(Path.Combine(musicRoot, "astropilotmusic.html")),
            ResourceId.Music_Cratediggers_Html => new FileInfo(Path.Combine(musicRoot, "cratediggers.html")),
            ResourceId.Music_Goataholicskjald_Html => new FileInfo(Path.Combine(musicRoot, "goataholicskjald.html")),
            ResourceId.Music_Mstrvlk_Html => new FileInfo(Path.Combine(musicRoot, "mstrvlk.html")),
            ResourceId.Music_Projectmooncircle_Html => new FileInfo(Path.Combine(musicRoot, "projectmooncircle.html")),
            ResourceId.Music_Tympanikaudio_Html => new FileInfo(Path.Combine(musicRoot, "tympanikaudio.html")),
            ResourceId.Music_Weneverlearnedtolive_Html => new FileInfo(Path.Combine(musicRoot, "weneverlearnedtolive.html")),
            _ => throw new ArgumentOutOfRangeException(nameof(resourceId), resourceId, null),
        };
    }
}
