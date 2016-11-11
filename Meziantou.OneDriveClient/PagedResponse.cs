using System.Collections.Generic;
using Newtonsoft.Json;

namespace Meziantou.OneDriveClient
{
    internal class PagedResponse<T>
    {
        public List<T> Value { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string NextLink { get; set; }
    }
}