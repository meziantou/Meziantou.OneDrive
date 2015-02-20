using System;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    public class Image
    {
        [JsonUtilities("height")]
        public int Height { get; protected set; }

        [JsonUtilities("width")]
        public int Width { get; protected set; }

        [JsonUtilities("source")]
        public Uri Source { get; protected set; }
        [JsonUtilities("type")]
        public ImageType Type { get; protected set; }
    }
}