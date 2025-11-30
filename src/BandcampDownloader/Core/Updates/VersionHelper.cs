using System;

namespace BandcampDownloader.Core.Updates;

internal static class VersionHelper
{
    public static bool IsNewerVersion(this Version version)
    {
        return Constants.AppVersion.CompareTo(version) < 0;
    }
}
