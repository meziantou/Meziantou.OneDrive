using System;

namespace Meziantou.OneDriveClient
{
    internal class ChunkedUploadSessionResult
    {
        public string UploadUrl { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public string[] NextExpectedRanges { get; set; }
    }
}