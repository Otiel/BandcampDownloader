using System;

namespace BandcampDownloader {

    internal class File {
        public String Url           { get; set; }
        public long   BytesReceived { get; set; }
        public long   Size          { get; set; }

        public File(String url, long bytesReceived, long size) {
            this.Url           = url;
            this.BytesReceived = bytesReceived;
            this.Size          = size;
        }
    }
}