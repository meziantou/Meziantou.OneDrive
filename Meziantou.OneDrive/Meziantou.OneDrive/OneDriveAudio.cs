using System;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    /// <summary>
    /// The Audio object contains info about a user's audio in Microsoft OneDrive.
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/en-us/library/dn631831.aspx</remarks> 
    public class OneDriveAudio : OneDriveFile
    {
        public OneDriveAudio(OneDriveClient client) : base(client)
        {
        }

        [JsonUtilities("picture", IgnoreWhenSerializing = true)]
        public Uri Picture { get; protected set; }
        [JsonUtilities("title")]
        public string Title { get; set; }

        [JsonUtilities("artist")]
        public string Artist { get; set; }
        [JsonUtilities("album")]
        public string Album { get; set; }
        [JsonUtilities("album_artist")]
        public string AlbumArtist { get; set; }
        [JsonUtilities("genre")]
        public string Genre { get; set; }
        [JsonUtilities("duration", IgnoreWhenSerializing = true)]
        public int Duration { get; set; }
    }
}