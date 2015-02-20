using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    /// <summary>
    /// The Video object contains info about a user's videos on Microsoft OneDrive.
    /// </summary>
    /// /// <remarks>https://msdn.microsoft.com/en-us/library/dn631849.aspx</remarks> 
    public class OneDriveVideo : OneDriveFile, ITaggable
    {
        public OneDriveVideo(OneDriveClient client) : base(client)
        {
        }

        [JsonUtilities("picture", IgnoreWhenSerializing = true)]
        public Uri Picture { get; protected set; }

        [JsonUtilities("tags_enabled", IgnoreWhenSerializing = true)]
        public bool TagsEnabled { get; protected set; }
        [JsonUtilities("tags_count", IgnoreWhenSerializing = true)]
        public int TagCount { get; protected set; }
        [JsonUtilities("height", IgnoreWhenSerializing = true)]
        public int Height { get; protected set; }
        [JsonUtilities("width", IgnoreWhenSerializing = true)]
        public int Width { get; protected set; }
        [JsonUtilities("duration", IgnoreWhenSerializing = true)]
        public int Duration { get; protected set; }
        [JsonUtilities("bitrate", IgnoreWhenSerializing = true)]
        public int BitRate { get; protected set; }

        public Task<IEnumerable<Tag>> GetTagsAsync(GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Client.GetCollectionAsync<Tag>(string.Format("/{0}/tags", Id), options, cancellationToken);
        }

        public Task<Tag> AddTagAsync(User user, float x, float y, CancellationToken cancellationToken = default(CancellationToken))
        {
            Tag tag = new Tag(Client, user, x, y);
            return Client.PostAsync<Tag>(string.Format("/{0}/tags", Id), tag, cancellationToken);
        }
    }
}