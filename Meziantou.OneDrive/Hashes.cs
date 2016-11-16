using System.Collections.Generic;
using Newtonsoft.Json;

namespace Meziantou.OneDrive
{
    public class Hashes
    {
        public string Sha1Hash { get; set; }
        public string Crc32Hash { get; set; }
        public string QuickXorHash { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}