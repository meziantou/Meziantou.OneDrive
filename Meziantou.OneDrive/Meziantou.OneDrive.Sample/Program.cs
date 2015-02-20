using System;
using System.IO;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive.Sample
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            const string clientId = "0000000000000000";
            IRefreshTokenHandler refreshTokenHandler = null;// new CredentialManagerRefreshTokenHandler("OneDriveSample");
            //var refreshTokenHandler = new InMemoryRefreshTokenHandler();
            var session = AuthenticationForm.CreateSessionAsync(clientId, new[]
            {
                Scope.BasicProfile,
                Scope.Photos,
                Scope.ReadOnly,
                Scope.ReadWrite,
                Scope.SharedItems,
                Scope.OfflineAccess
            }, refreshTokenHandler).Result;

            if (session == null)
                return;

            OneDriveClient client = new OneDriveClient(session);

            Quota quota = client.GetQuotaAsync().Result;
            Console.WriteLine("Quota: {0} free of {1} ({2} used)",
                ConvertUtilities.FormatFileSize(quota.TotalSpace),
                ConvertUtilities.FormatFileSize(quota.AvailableSpace),
                ConvertUtilities.FormatFileSize(quota.TotalSpace - quota.AvailableSpace));

            OneDriveFolder root = client.GetOneDriveRootAsync().Result;
            Console.WriteLine("Root Item: " + root.Name);
            Console.WriteLine("CameraRoll Item: " + client.GetWellKnownFolderAsync(WellKnownFolder.CameraRoll).Result?.Name);
            Console.WriteLine("Documents Item: " + client.GetWellKnownFolderAsync(WellKnownFolder.Documents).Result?.Name);
            Console.WriteLine("Pictures Item: " + client.GetWellKnownFolderAsync(WellKnownFolder.Pictures).Result?.Name);
            var publicDocuments = client.GetWellKnownFolderAsync(WellKnownFolder.PublicDocuments).Result;
            Console.WriteLine("PublicDocuments Item: " + publicDocuments?.Name);


            foreach (OneDriveItem child in root.GetChildrenAsync().Result)
            {
                Console.WriteLine("-- " + child.Name);
                if (child.Type == ItemType.Folder && child.Name.StartsWith("Demo"))
                {
                    child.DeleteAsync().Wait();
                }
            }

            // paging
            Console.WriteLine("Paging");
            foreach (OneDriveItem child in root.GetChildrenAsync(new GetOptions() { Offset = 1, Limit = 2 }).Result)
            {
                Console.WriteLine("-- " + child.Name);
            }

            // Item type filter
            Console.WriteLine("ItemTypeFilter: Folders");
            foreach (OneDriveItem child in root.GetChildrenAsync(new GetOptions() { Filters = ItemTypeFilter.Folder }).Result)
            {
                Console.WriteLine("-- " + child.Name);
            }

            Console.WriteLine("ItemTypeFilter: Photo");
            foreach (OneDriveItem child in root.GetChildrenAsync(new GetOptions() { Filters = ItemTypeFilter.Photo }).Result)
            {
                Console.WriteLine("-- " + child.Name);
            }

            Console.WriteLine("ItemTypeFilter: Photos,Audio,Albums");
            foreach (OneDriveItem child in root.GetChildrenAsync(new GetOptions() { Filters = ItemTypeFilter.Photo | ItemTypeFilter.Audio | ItemTypeFilter.Album }).Result)
            {
                Console.WriteLine("-- " + child.Name);
            }

            Console.WriteLine("Search");
            foreach (OneDriveItem child in client.SearchAsync("barre", new GetOptions()).Result)
            {
                Console.WriteLine("-- " + child.Name);
            }

            Console.WriteLine("Search & Paging");
            foreach (OneDriveItem child in client.SearchAsync("barre", new GetOptions() { Offset = 0, Limit = 1 }).Result)
            {
                Console.WriteLine("-- " + child.Name);
            }


            Console.WriteLine("Recents");
            foreach (OneDriveItem child in client.GetRecentItemsAsync().Result)
            {
                Console.WriteLine(" " + child.Name);
            }
            Console.WriteLine("Shared");
            foreach (OneDriveItem child in client.GetSharedItemsAsync().Result)
            {
                Console.WriteLine(" " + child.Name + "(" + child?.From.Name + ")");
            }

            OneDriveFolder newFolder = root.CreateChildDirectoryAsync("DemoNewFolder").Result;
            newFolder.Name = "DemoNewFolderRenamed";
            newFolder.SaveAsync().Wait();
            foreach (OneDriveItem child in root.GetChildrenAsync().Result)
            {
                Console.WriteLine("-- " + child.Name);
            }
            Console.WriteLine("Uploading file");
            FileInfo fileInfo = new FileInfo(@"C:\Users\meziantou\Documents\script.sql");

            using (FileStream fileStream = fileInfo.OpenRead())
            {
                newFolder.BitsUploadAsync("script.sql", fileStream).Wait();
            }
            var file = newFolder.UploadAsync(fileInfo, OverwriteOption.Rename).Result;

            Console.WriteLine("Uploaded: " + file.Name);
            using (Stream stream = file.DownloadAsync().Result)
            {
                using (Stream fs = File.OpenWrite(@"C:\Users\meziantou\Documents\script2.sql"))
                {
                    stream.CopyTo(fs);
                }
            }

            // Range Download
            using (Stream stream = file.DownloadAsync(rangeStart: 0, rangeEnd: 10).Result)
            {
                using (Stream fs = File.OpenWrite(@"C:\Users\meziantou\Documents\script3.sql"))
                {
                    stream.CopyTo(fs);
                }
            }
            using (Stream stream = file.DownloadAsync(11).Result)
            {
                using (Stream fs = File.Open(@"C:\Users\meziantou\Documents\script3.sql", FileMode.Append, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                }
            }

            Console.WriteLine(new FileInfo(@"C:\Users\meziantou\Documents\script2.sql").Length == fileInfo.Length ? "Upload/Download OK" : "Upload/Download failed");
            Console.WriteLine("Read Link: " + file.GetLinkAsync(LinkType.ReadOnly).Result);
            Console.WriteLine("Edit Link: " + file.GetLinkAsync(LinkType.ReadWrite).Result);
            Console.WriteLine("Embed Link: " + file.GetLinkAsync(LinkType.Embed).Result);

            var moveToFolder = root.CreateChildDirectoryAsync("DemoNewFolderRoot").Result;
            newFolder.MoveToNewParentAsync(moveToFolder).Wait();
            file.CopyToNewParentAsync(moveToFolder).Wait();

            newFolder.DeleteAsync().Wait();
            moveToFolder.DeleteAsync().Wait();
        }
    }
}
