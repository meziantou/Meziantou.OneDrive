using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meziantou.OneDrive
{
    public class OneDriveFile : OneDriveItem
    {
        public OneDriveFile(OneDriveClient client) : base(client)
        {
        }

        public Task CopyToNewParentAsync(OneDriveItem parentItem, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (parentItem == null) throw new ArgumentNullException(nameof(parentItem));

            return Client.CopyAsync(string.Format("/{0}", Id), parentItem.Id, cancellationToken);
        }

        public Task<Stream> DownloadAsync(long? rangeStart = null, long? rangeEnd = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Client.DowloadAsync(string.Format("/{0}/content", Id), rangeStart, rangeEnd, cancellationToken);
        }
    }
}