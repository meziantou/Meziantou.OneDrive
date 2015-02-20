using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meziantou.OneDrive
{
    public class OneDriveFolder : OneDriveItem
    {
        public OneDriveFolder(OneDriveClient client) : base(client)
        {
        }

        public Task<IEnumerable<OneDriveItem>> GetChildrenAsync(GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Client.GetCollectionAsync<OneDriveItem>(string.Format("/{0}/files", Id), options, cancellationToken);
        }

        public Task<OneDriveFolder> CreateChildDirectoryAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Client.PostAsync<OneDriveFolder>(string.Format("/{0}", Id), new Dictionary<string, object> { { "name", name } }, cancellationToken);
        }

        public async Task<OneDriveFile> UploadAsync(FileInfo fileInfo, OverwriteOption overwriteOption, bool resizePhoto = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            using (FileStream stream = fileInfo.OpenRead())
            {
                return await UploadAsync(fileInfo.Name, stream, overwriteOption, resizePhoto, cancellationToken);
            }
        }

        public async Task<OneDriveFile> UploadAsync(string name, Stream steam, OverwriteOption overwriteOption, bool resizePhoto = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (steam == null) throw new ArgumentNullException(nameof(steam));

            Dictionary<string, object> result = await Client.UploadAsync<Dictionary<string, object>>(UploadLocation.AbsoluteUri, name, steam, overwriteOption, resizePhoto, cancellationToken);
            // {
            //  "id": "file.f805b757667f2641.F905B757667F1656!102449",
            //  "name": "sample.txt",
            //  "source": "https://xxx.livefilestore.com/..."
            // }
            object o;
            if (result == null || !result.TryGetValue("id", out o))
                return null;

            string id = o as string;
            if (id == null)
                return null;

            return await Client.GetItemByIdAsync(id, cancellationToken).ConfigureAwait(false) as OneDriveFile;
        }

        public async Task<OneDriveFile> BitsUploadAsync(FileInfo fileInfo, int chunckLength = 4096 * 1024/*4MO*/, EventHandler<BitsUploadChunckFailedEventArgs> chunckFailedCallback = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            using (Stream stream = fileInfo.OpenRead())
            {
                return await BitsUploadAsync(fileInfo.Name, stream, chunckLength, chunckFailedCallback, cancellationToken);
            }
        }

        public async Task<OneDriveFile> BitsUploadAsync(string name, Stream steam, int chunckLength = 4096 * 1024/*4MO*/, EventHandler<BitsUploadChunckFailedEventArgs> chunckFailedCallback = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (steam == null) throw new ArgumentNullException(nameof(steam));

            string fileId = await Client.BitsUploadAsync(From.Id, Id, name, steam, chunckLength, chunckFailedCallback, cancellationToken).ConfigureAwait(false);
            OneDriveFile file = await Client.GetItemByIdAsync(fileId, cancellationToken).ConfigureAwait(false) as OneDriveFile;
            return file;
        }

    }
}