using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    /// <summary>
    /// The Tag object contains info about tags that are associated with a photo or a video on Microsoft OneDrive.
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/en-us/library/dn631848.aspx</remarks>
    public class Tag
    {
        private readonly OneDriveClient _client;

        public Tag(OneDriveClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = client;
        }

        public Tag(OneDriveClient client, User user, float x, float y)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (user == null) throw new ArgumentNullException(nameof(user));
            _client = client;
            User = user;
            X = x;
            Y = y;
        }

        protected OneDriveClient Client
        {
            get { return _client; }
        }

        [JsonUtilities("id")]
        public string Id { get; protected set; }

        [JsonUtilities("user")]
        public User User { get; set; }

        [JsonUtilities("x")]
        public float X { get; set; }

        [JsonUtilities("y")]
        public float Y { get; set; }
        [JsonUtilities("created_time", IgnoreWhenSerializing = true)]
        public DateTime CreatedTime { get; protected set; }

        public async Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Dictionary<string, object> result = await Client.PutAsync<Dictionary<string, object>>(string.Format("/{0}", Id), this, cancellationToken);
            Client.Apply(result, this);
        }

        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Client.DeleteAsync(string.Format("/{0}", Id), cancellationToken);
        }

    }
}