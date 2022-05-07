using System.IO;
using System.IO.Compression;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSaveFiles.Reader;

namespace ManageModsAndSaveFiles.Compression.Microsoft;

public sealed class ZipFileHandle : DumpableObject, IZipFileHandle
{
    readonly ValueCache<ZipArchiveEntry> ZipArchiveEntryCache;
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

    [DisableDump]
    string IZipFileHandle.String
    {
        get
        {
            (!string.IsNullOrEmpty(ItemPath)).Assert();
            return new BinaryRead(Reader).GetNextString((int)Length);
        }
    }

    protected override string GetNodeDump() => Archive.Path + "+" + ItemPath;

    long Length => Profiler.Measure(() => ZipArchiveEntryCache.Value.Length);

    Stream Reader
    {
        get
        {
            var zipEntry = ZipArchiveEntryCache.Value;
            var zipReader = zipEntry.Open();
            var reader = new SeekableReader(zipReader, Length);

            return new StreamWithCleanupList(reader, reader, zipReader);
        }
    }


    ZipArchiveEntry GetZipArchiveEntry()
        => Profiler.Measure(() => Archive.GetZipArchiveEntry(ItemPath));
}