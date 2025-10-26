﻿using System;
using System.Reflection;

namespace BandcampDownloader.Updates;

public static class VersionHelper
{
    public static Version GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version;
    }

    public static bool IsNewerVersion(this Version version)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        return currentVersion!.CompareTo(version) < 0;
    }
}
