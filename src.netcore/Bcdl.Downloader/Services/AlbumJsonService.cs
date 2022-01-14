using System.Net;
using System.Text.RegularExpressions;
using Serilog;

namespace Bcdl.Downloader.Services;

public interface IAlbumJsonService
{
    string ExtractAlbumJson(string albumHtml);
}

public sealed class AlbumJsonService : IAlbumJsonService
{
    private readonly ILogger _logger;
    private const string START_STRING = "data-tralbum=\"{";
    private const string END_STRING = "}\"";

    public AlbumJsonService(ILogger logger)
    {
        _logger = logger;
    }

    public string ExtractAlbumJson(string albumHtml)
    {
        var capturingGroup = "jsonContent";
        var regex = new Regex($"{START_STRING}(?<{capturingGroup}>.*?){END_STRING}");
        var match = regex.Match(albumHtml);
        if (!match.Success)
        {
            _logger.Error("Could not extract album json from {0}", nameof(albumHtml));
            throw new ArgumentException("Could not extract album json", nameof(albumHtml));
        }

        var htmlEncodedJson = $"{{{match.Groups[capturingGroup].Value}}}"; // Need to add braces as they're a part of START_STRING and END_STRING, thus not included in the captured group

        var json = WebUtility.HtmlDecode(htmlEncodedJson);

        return json;
    }
}
