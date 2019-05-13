namespace BandcampDownloader {

    internal class TrackFile {
        public long BytesReceived { get; set; }
        public bool Downloaded { get; set; }
        public long Size { get; set; }
        public string Url { get; set; }

        public TrackFile(string url, long bytesReceived, long size) {
            Url = url;
            BytesReceived = bytesReceived;
            Size = size;
        }
    }
}