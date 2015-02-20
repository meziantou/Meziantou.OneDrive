using System.ComponentModel;

namespace Meziantou.OneDrive
{
    public class BitsUploadChunckFailedEventArgs : CancelEventArgs
    {
        public BitsUploadChunckFailedEventArgs(string folderId, string fileName, long rangeStart, long rangeEnd, long totalLength, int attempt)
        {
            FolderId = folderId;
            FileName = fileName;
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
            TotalLength = totalLength;
            Attempt = attempt;
        }

        string FolderId { get; }
        string FileName { get; }
        public long RangeStart { get; }
        public long RangeEnd { get; }
        public long TotalLength { get; }
        public int Attempt { get; }
    }
}