using System.IO;
using System.Threading.Tasks;

namespace BandcampDownloader.IO;

internal interface IFileService
{
    /// <summary>
    /// Asynchronously copies an existing file to a new file.
    /// </summary>
    /// <param name="sourceFile">The file to copy.</param>
    /// <param name="destinationFile">The name of the destination file.</param>
    Task CopyFileAsync(string sourceFile, string destinationFile);
}

internal sealed class FileService : IFileService
{
    public async Task CopyFileAsync(string sourceFile, string destinationFile)
    {
        // https://stackoverflow.com/a/35467471/825024
        const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        const int bufferSize = 4096;

        await using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions);
        await using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions);
        await sourceStream.CopyToAsync(destinationStream);
    }
}
