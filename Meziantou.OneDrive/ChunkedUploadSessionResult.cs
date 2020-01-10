using System;

namespace Meziantou.OneDrive
{
    internal sealed class ChunkedUploadSessionResult
    {
        public string UploadUrl { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public string[] NextExpectedRanges { get; set; }
    }
}