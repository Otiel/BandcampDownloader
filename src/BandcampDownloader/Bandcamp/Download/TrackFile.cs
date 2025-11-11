namespace BandcampDownloader.Bandcamp.Download;

internal sealed class TrackFile
{
    public long BytesReceived { get; set; }
    public bool Downloaded { get; set; }
    public long Size { get; }
    public string Url { get; }

    public TrackFile(string url, long bytesReceived, long size)
    {
        Url = url;
        BytesReceived = bytesReceived;
        Size = size;
    }
}
