using System.Diagnostics;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    [DebuggerDisplay("Total space: {TotalSpace}; Available space: {AvailableSpace}")]
    public class Quota
    {
        [JsonUtilities("quota")]
        public long TotalSpace { get; set; }

        [JsonUtilities("available")]
        public long AvailableSpace { get; set; }
    }
}