using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    /// <summary>
    /// The Photo object contains info about a user's photos on Microsoft OneDrive.
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/en-us/library/dn631841.aspx</remarks>
    public class OneDrivePhoto : OneDriveFile, ITaggable
    {
        [JsonUtilities("picture", IgnoreWhenSerializing = true)]
        public Uri Picture { get; protected set; }

        [JsonUtilities("tags_enabled", IgnoreWhenSerializing = true)]
        public bool TagsEnabled { get; protected set; }
        [JsonUtilities("tags_count", IgnoreWhenSerializing = true)]
        public int TagCount { get; protected set; }
        [JsonUtilities("images", IgnoreWhenSerializing = true)]
        public Image[] Images { get; protected set; }

        [JsonUtilities("height", IgnoreWhenSerializing = true)]
        public int Height { get; protected set; }

        [JsonUtilities("width", IgnoreWhenSerializing = true)]
        public int Width { get; protected set; }

        [JsonUtilities("when_taken", IgnoreWhenSerializing = true)]
        public DateTime WhenTaken { get; protected set; }

        [JsonUtilities("location", IgnoreWhenSerializing = true)]
        public Location Location { get; protected set; }

        [JsonUtilities("camera_make", IgnoreWhenSerializing = true)]
        public string CameraMake { get; protected set; }
        [JsonUtilities("camera_model", IgnoreWhenSerializing = true)]
        public string CameraModel { get; protected set; }
        [JsonUtilities("focal_ratio", IgnoreWhenSerializing = true)]
        public double FocalRatio { get; protected set; }
        [JsonUtilities("focal_length", IgnoreWhenSerializing = true)]
        public double FocalLength { get; protected set; }
        [JsonUtilities("exposure_numerator", IgnoreWhenSerializing = true)]
        public double ExposureNumerator { get; protected set; }
        [JsonUtilities("exposure_denominator", IgnoreWhenSerializing = true)]
        public double ExposureDenominator { get; protected set; }


        public OneDrivePhoto(OneDriveClient client) : base(client)
        {
        }


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