using ATL.Logging;

namespace BandcampDownloader.Audio.AtlDotNet;

public interface IAtlLogInterceptor
{
    void Initialize();
}

/// <summary>
/// ATL.NET does not throw exceptions, instead it fails silently (on purpose).
/// See :
/// • https://github.com/Zeugma440/atldotnet/wiki/4.-FAQ-(Frequently-Asked-Questions)#theres-something-wrong-with-the-library-but-i-have-no-useful-output-on-the-console-why-so-
/// • https://github.com/Zeugma440/atldotnet/wiki/3.-Usage-_-Code-snippets#receiving-atl-logs-in-your-app
/// This class aims to intercept the ATL.NET logs in order to throw an exception on Error logs.
/// </summary>
public class AtlLogInterceptor : ILogDevice, IAtlLogInterceptor
{
    private Log _myLog = new();

    public void Initialize()
    {
        LogDelegator.SetLog(ref _myLog);
        _myLog.Register(this);
    }

    public void DoLog(Log.LogItem logItem)
    {
        if (logItem.Level == 1) // ERROR
        {
            throw new AtlException($"{logItem.Message} for file {logItem.Location}");
        }
    }
}
