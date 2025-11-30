using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BandcampDownloader.IO;

internal interface IFileService
{
    Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken);
    Task SaveStreamToFileAsync(Stream stream, string destinationFile, CancellationToken cancellationToken);
}

internal sealed class FileService : IFileService
{
    public async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken)
    {
        // https://stackoverflow.com/a/35467471/825024
        const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        const int bufferSize = 4096;

        await using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions);
        await using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveStreamToFileAsync(Stream stream, string destinationFile, CancellationToken cancellationToken)
    {
        stream.Position = 0;
        await using var fileStream = new FileStream(destinationFile, FileMode.Create);
        await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
    }
}
