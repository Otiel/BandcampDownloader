using System;

namespace BandcampDownloader.Audio.AtlDotNet;

public class AtlException : Exception
{
    public AtlException(string message) : base(message)
    {
    }
}
