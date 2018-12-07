using System;

namespace BandcampDownloader {

    internal class DownloadProgress {
        public long BytesReceived { get; set; }
        public String FileUrl { get; set; }

        public DownloadProgress(String fileUrl, long bytesReceived) {
            FileUrl = fileUrl;
            BytesReceived = bytesReceived;
        }
    }
}