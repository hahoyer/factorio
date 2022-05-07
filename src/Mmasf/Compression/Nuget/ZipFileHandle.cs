using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ICSharpCode.SharpZipLib.Zip;
using ManageModsAndSaveFiles.Reader;

namespace ManageModsAndSaveFiles.Compression.Nuget;

public sealed class ZipFileHandle : DumpableObject, IZipFileHandle
{
    readonly ValueCache<ZipEntry> ZipArchiveEntryCache;
    readonly ZipArchiveHandle Archive;
    readonly string ItemPath;

    internal ZipFileHandle(ZipArchiveHandle archive, string itemPath)
    {
        Archive = archive;
        ItemPath = itemPath;
        ZipArchiveEntryCache = new(GetZipArchiveEntry);
    }

    int IZipFileHandle.Depth => ItemPath.Split('/').Length;

    string IZipFileHandle.ItemName => ItemPath.Split('/').Last();

    Stream IZipFileHandle.Reader => Reader;

    string IZipFileHandle.String
    {
        get
        {
            (!string.IsNullOrEmpty(ItemPath)).Assert();
            return new BinaryRead(Reader).GetNextString((int)Length);
        }
    }

    protected override string GetNodeDump() => Archive.Path + "+" + ItemPath;

    long Length => ZipArchiveEntryCache.Value.Size;

    Stream Reader
    {
        get
        {
            var zipEntry = ZipArchiveEntryCache.Value;
            var zipReader = Archive.ZipArchive.GetInputStream(zipEntry);
            var reader = new SeekableReader(zipReader, Length);
            return new StreamWithCleanupList(reader, reader, zipReader);
        }
    }

    ZipEntry GetZipArchiveEntry() => Archive.GetZipArchiveEntry(ItemPath);
}