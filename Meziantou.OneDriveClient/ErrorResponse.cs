using System.Collections.Generic;
using Newtonsoft.Json;

namespace Meziantou.OneDriveClient
{
    public class ErrorResponse
    {
        public Error Error { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}