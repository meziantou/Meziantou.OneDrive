using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    public sealed class SharingAccessScope
    {
        [JsonUtilities("access")]
        public string Access { get; set; }
    }
}