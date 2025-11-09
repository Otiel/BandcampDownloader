using Downloader;

namespace BandcampDownloader.IntegrationTests;

public sealed class BezzadDownloaderTests
{
    private DownloadService _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new DownloadService();
    }

    [Test]
    [Ignore("Test fails")]
    public void RequestingCancellation_Throw_OperationCanceledException()
    {
        // Arrange
        const string fileUrl = "https://www.examplefile.com/file-download/202"; // 1 GB txt file
        var cts = new CancellationTokenSource();
        cts.CancelAfter(500);

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            // Act
            await _sut.DownloadFileTaskAsync(fileUrl, cts.Token);
        });
    }

    [TearDown]
    public void Cleanup()
    {
        _sut.Dispose();
    }
}
