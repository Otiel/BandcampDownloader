// ReSharper disable StringLiteralTypo

namespace BandcampDownloader.UnitTests.Resources;

internal static class ResourceAccessor
{
    public static FileInfo GetFileInfo(ResourceId resourceId)
    {
        var resourcesFilesRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "Files");

        return resourceId switch
        {
            ResourceId.AffektrecordingsHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "affektrecordings.html")),
            ResourceId.AfterdarkrecordingsHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "afterdarkrecordings.html")),
            ResourceId.CratediggersHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "cratediggers.html")),
            ResourceId.GoataholicskjaldHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "goataholicskjald.html")),
            ResourceId.MstrvlkHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "mstrvlk.html")),
            ResourceId.ProjectmooncircleHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "projectmooncircle.html")),
            ResourceId.TympanikaudioHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "tympanikaudio.html")),
            ResourceId.WeneverlearnedtoliveHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "weneverlearnedtolive.html")),
            _ => throw new ArgumentOutOfRangeException(nameof(resourceId), resourceId, null),
        };
    }
}
