using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BandcampDownloader.Helpers;

public static class StreamExtensions
{
    public static async Task<byte[]> ToArrayAsync(this Stream stream, CancellationToken cancellationToken)
    {
        await using var memoryStream = new MemoryStream();

        stream.Position = 0;
        await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);

        return memoryStream.ToArray();
    }
}
