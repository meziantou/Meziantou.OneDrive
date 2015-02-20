using System;
using System.Collections.Generic;
using System.Linq;
using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    internal static class OneDriveUtilities
    {
        public static string GetScopeName(Scope scope)
        {
            switch (scope)
            {
                case Scope.BasicProfile:
                    return "wl.basic";
                case Scope.Photos:
                    return "wl.photos";
                case Scope.ReadOnly:
                    return "wl.skydrive";
                case Scope.ReadWrite:
                    return "wl.skydrive_update";
                case Scope.SharedItems:
                    return "wl.contacts_skydrive";
                case Scope.OfflineAccess:
                    return "wl.offline_access";
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }

        public static string GetScopeName(IEnumerable<Scope> scopes)
        {
            return string.Join(",", scopes.Select(GetScopeName));
        }

        public static string GetWellKnowFolderPath(WellKnownFolder wellKnownFolder)
        {
            switch (wellKnownFolder)
            {
                case WellKnownFolder.CameraRoll:
                    return "/skydrive/camera_roll";
                case WellKnownFolder.Documents:
                    return "/skydrive/my_documents";
                case WellKnownFolder.Pictures:
                    return "/skydrive/my_photos";
                case WellKnownFolder.PublicDocuments:
                    return "/skydrive/public_documents";
                default:
                    throw new ArgumentOutOfRangeException(nameof(wellKnownFolder));
            }
        }

        public static string GetLinkTypePath(LinkType linkType)
        {
            switch (linkType)
            {
                case LinkType.Embed:
                    return "embed";
                case LinkType.ReadOnly:
                    return "shared_read_link";
                case LinkType.ReadWrite:
                    return "shared_edit_link";
                default:
                    throw new ArgumentOutOfRangeException(nameof(linkType));
            }
        }

        public static ItemType GetItemType(string type)
        {
            if (type == null)
                return ItemType.Unknown;

            switch (type.ToLowerInvariant())
            {
                case "folder":
                    return ItemType.Folder;
                case "album":
                    return ItemType.Album;
                case "file":
                    return ItemType.File;
                case "photo":
                    return ItemType.Photo;
                case "video":
                    return ItemType.Video;
                case "audio":
                    return ItemType.Audio;
                case "notebook":
                    return ItemType.Notebook;
                default:
                    return ItemType.Unknown;
            }
        }
        public static string GetFilterName(ItemTypeFilter type)
        {
            var filters = ConvertUtilities.SplitEnumValues(type);
            return string.Join(",", filters.Select(GetSingleFilterName));
        }

        private static string GetSingleFilterName(ItemTypeFilter filter)
        {
            switch (filter)
            {
                case ItemTypeFilter.None:
                    return null;
                case ItemTypeFilter.Album:
                    return "albums";
                case ItemTypeFilter.Audio:
                    return "audio";
                case ItemTypeFilter.Folder:
                    return "folders";
                case ItemTypeFilter.NoteBook:
                    return "notebooks";
                case ItemTypeFilter.Photo:
                    return "photos";
                case ItemTypeFilter.Video:
                    return "videos";
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }

        public static string GetSortPropertyName(SortProperty type)
        {
            switch (type)
            {
                case SortProperty.Default:
                    return "default";
                case SortProperty.Name:
                    return "name";
                case SortProperty.Updated:
                    return "updated";
                case SortProperty.Size:
                    return "size";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
