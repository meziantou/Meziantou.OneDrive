using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    public static class Extensions
    {
        //public static Quota GetQuota(this OneDriveClient client)
        //{
        //    return AsyncPump.RunSync(async () => await client.GetQuotaAsync());
        //}

        //public static OneDriveItem GetOneDriveRoot(this OneDriveClient client)
        //{
        //    return AsyncPump.RunSync(async () => await client.GetOneDriveRootAsync());
        //}
        //public static OneDriveItem GetWellKonwFolder(this OneDriveClient client, WellKnownFolder wellKnownFolder)
        //{
        //    return AsyncPump.RunSync(async () => await client.GetWellKonwFolderAsync(wellKnownFolder));
        //}

        //public static IEnumerable<OneDriveItem> GetRecentItems(this OneDriveClient client)
        //{
        //    return AsyncPump.RunSync(async () => await client.GetRecentItemsAsync());
        //}

        //public static IEnumerable<OneDriveItem> GetSharedItems(this OneDriveClient client)
        //{
        //    return AsyncPump.RunSync(async () => await client.GetSharedItemsAsync());
        //}

        //public static IEnumerable<OneDriveItem> GetChildren(this OneDriveFolder oneDriveItem)
        //{
        //    return AsyncPump.RunSync(async () => await oneDriveItem.GetChildrenAsync());
        //}

        //public static OneDriveItem CreateChildDirectory(this OneDriveFolder oneDriveItem, string name)
        //{
        //    return AsyncPump.RunSync(async () => await oneDriveItem.CreateChildDirectoryAsync(name));
        //}

        //public static void Delete(this OneDriveItem oneDriveItem)
        //{
        //    AsyncPump.RunSync(async () => await oneDriveItem.DeleteAsync());
        //}
        //public static void Save(this OneDriveItem oneDriveItem)
        //{
        //    AsyncPump.RunSync(async () => await oneDriveItem.SaveAsync());
        //}
        //public static void MoveToNewParent(this OneDriveItem oneDriveItem, OneDriveItem parentItem)
        //{
        //    AsyncPump.RunSync(async () => await oneDriveItem.MoveToNewParentAsync(parentItem));
        //}
        //public static void CopyToNewParent(this OneDriveFile oneDriveItem, OneDriveItem parentItem)
        //{
        //    AsyncPump.RunSync(async () => await oneDriveItem.CopyToNewParentAsync(parentItem));
        //}
        //public static OneDriveItem Upload(this OneDriveItem oneDriveItem, FileInfo fileInfo, OverwriteOption overwriteOption, bool resizePhoto = false, IProgress<ProgressInfo> progress = null)
        //{
        //    return AsyncPump.RunSync(async () => await oneDriveItem.UploadAsync(fileInfo, overwriteOption, resizePhoto, progress));
        //}
        //public static Stream Download(this OneDriveItem oneDriveItem)
        //{
        //    return AsyncPump.RunSync(async () => await oneDriveItem.DownloadAsync());
        //}
        //public static Stream Download(this OneDriveItem oneDriveItem, IProgress<ProgressInfo> progress)
        //{
        //    return AsyncPump.RunSync(async () => await oneDriveItem.DownloadAsync(progress));
        //}
        //public static string GetLink(this OneDriveItem oneDriveItem, LinkType linkType)
        //{
        //    return AsyncPump.RunSync(async () => await oneDriveItem.GetLinkAsync(linkType));
        //}
        //public static OneDriveItem BitsUpload(this OneDriveItem oneDriveItem, string name, Stream stream)
        //{
        //    return AsyncPump.RunSync(async () => await oneDriveItem.BitsUploadAsync(name, stream));
        //}

        internal static string GetValue(this HttpResponseHeaders headers, string name)
        {
            return GetValue(headers, name, (string)null);
        }

        internal static T GetValue<T>(this HttpResponseHeaders headers, string name, T defaultValue)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            IEnumerable<string> values;
            if (!headers.TryGetValues(name, out values))
                return defaultValue;

            foreach (string value in values)
            {
                return ConvertUtilities.ChangeType(value, defaultValue);
            }

            return defaultValue;
        }
        internal static TResult GetValue<TKey, TValue, TResult>(this IDictionary<TKey, TValue> dict, TKey name, TResult defaultValue)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));

            TValue value;
            if (!dict.TryGetValue(name, out value))
                return defaultValue;

            return ConvertUtilities.ChangeType(value, defaultValue);
        }
    }
}