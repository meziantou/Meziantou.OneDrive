using System;

namespace Meziantou.OneDrive
{
    [Flags]
    public enum ItemTypeFilter
    {
        None = 0x0,
        Folder = 0x1,
        Album = 0x2,
        Photo = 0x4,
        Video = 0x8,
        Audio = 0x10,
        NoteBook = 0x20
    }
}