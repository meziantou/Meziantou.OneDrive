using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    public class OneDriveItem
    {
        public OneDriveItem(OneDriveClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            Client = client;
        }

        [JsonUtilities(IgnoreWhenSerializing = true, IgnoreWhenDeserializing = true)]
        protected OneDriveClient Client { get; }

        [JsonUtilities("id", IgnoreWhenSerializing = true)]
        public string Id { get; protected set; }

        [XmlAttribute("name")]
        [JsonUtilities("name")]
        public string Name { get; set; }

        [XmlAttribute("description")]
        [JsonUtilities("description")]
        public string Description { get; set; }

        [JsonUtilities("parent_id", IgnoreWhenSerializing = true)]
        public string ParentId { get; protected set; }

        [JsonUtilities("size", IgnoreWhenSerializing = true)]
        public long Size { get; protected set; }

        [JsonUtilities("upload_location", IgnoreWhenSerializing = true)]
        public Uri UploadLocation { get; protected set; }

        [JsonUtilities("comments_count", IgnoreWhenSerializing = true)]
        public int CommentsCount { get; protected set; }

        [JsonUtilities("comments_enabled", IgnoreWhenSerializing = true)]
        public bool CommentsEnabled { get; protected set; }

        [JsonUtilities("is_embeddable", IgnoreWhenSerializing = true)]
        public bool IsEmbeddable { get; protected set; }

        [JsonUtilities("count", IgnoreWhenSerializing = true)]
        public long ChildItemCount { get; protected set; }

        [JsonUtilities("link", IgnoreWhenSerializing = true)]
        public Uri Link { get; protected set; }

        [JsonUtilities("type", IgnoreWhenSerializing = true)]
        public string RawType { get; protected set; }

        [JsonUtilities(IgnoreWhenSerializing = true, IgnoreWhenDeserializing = true)]
        public ItemType Type
        {
            get
            {
                return OneDriveUtilities.GetItemType(RawType);
            }
        }

        [JsonUtilities("shared_with", IgnoreWhenSerializing = true)]
        public SharingAccessScope SharedWith { get; protected set; }

        [JsonUtilities("created_time", IgnoreWhenSerializing = true)]
        public DateTime CreatedTimeUtc { get; protected set; }

        [JsonUtilities("updated_time", IgnoreWhenSerializing = true)]
        public DateTime UpdatedTimeUtc { get; protected set; }

        [JsonUtilities("client_updated_time", IgnoreWhenSerializing = true)]
        public DateTime ClientUpdatedTime { get; protected set; }

        [JsonUtilities("from", IgnoreWhenSerializing = true)]
        public User From { get; protected set; }
        [JsonUtilities("source", IgnoreWhenSerializing = true)]
        public Uri Source { get; protected set; }

        public async Task<string> GetLinkAsync(LinkType linkType, CancellationToken cancellationToken = default(CancellationToken))
        {
            string linkTypeString = OneDriveUtilities.GetLinkTypePath(linkType);
            Dictionary<string, object> result = await Client.GetAsync<Dictionary<string, object>>(string.Format("/{0}/{1}", Id, linkTypeString), null, cancellationToken);
            object link;
            if (result.TryGetValue("link", out link))
                return link as string;

            if (result.TryGetValue("embed_html", out link))
                return link as string;

            return null;
        }

        //public Task<string> GetPreviewAsync(CancellationToken cancellationToken)
        //{
        //    // https://msdn.microsoft.com/en-us/library/dn659743.aspx#display_a_preview_of_a_onedrive_item
        //    throw new NotImplementedException();
        //}

        public async Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Dictionary<string, object> result = await Client.PutAsync<Dictionary<string, object>>(string.Format("/{0}", Id), this, cancellationToken);
            Client.Apply(result, this);
        }

        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Client.DeleteAsync(string.Format("/{0}", Id), cancellationToken);
        }

        public Task MoveToNewParentAsync(OneDriveItem parentItem, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (parentItem == null) throw new ArgumentNullException(nameof(parentItem));

            return Client.MoveAsync(string.Format("/{0}", Id), parentItem.Id, cancellationToken);
        }        
    }
}
