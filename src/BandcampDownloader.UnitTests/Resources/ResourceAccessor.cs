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
            ResourceId.ProjectmooncircleHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "projectmooncircle.html")),
            ResourceId.TympanikaudioHtml => new FileInfo(Path.Combine(resourcesFilesRoot, "tympanikaudio.html")),
            _ => throw new ArgumentOutOfRangeException(nameof(resourceId), resourceId, null),
        };
    }
}
