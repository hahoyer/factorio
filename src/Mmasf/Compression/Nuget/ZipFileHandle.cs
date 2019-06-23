using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ICSharpCode.SharpZipLib.Zip;
using ManageModsAndSaveFiles.Reader;

namespace ManageModsAndSaveFiles.Compression.Nuget
{
    public sealed class ZipFileHandle : DumpableObject, IZipFileHandle
    {
        readonly ValueCache<ZipEntry> ZipArchiveEntryCache;
        readonly ZipArchiveHandle Archive;
        readonly string ItemPath;

        string IZipFileHandle.ItemName => ItemPath.Split('/').Last();
        int IZipFileHandle.Depth => ItemPath.Split('/').Length;

        internal ZipFileHandle(ZipArchiveHandle archive, string itemPath)
        {
            Archive = archive;
            ItemPath = itemPath;
            ZipArchiveEntryCache = new ValueCache<ZipEntry>(GetZipArchiveEntry);
        }

        string IZipFileHandle.String
        {
            get
            {
                Tracer.Assert(!string.IsNullOrEmpty(ItemPath));
                return new BinaryRead(Reader).GetNextString((int) Length);
            }
        }

        long Length => ZipArchiveEntryCache.Value.Size;

        ZipEntry GetZipArchiveEntry() => Archive.GetZipArchiveEntry(ItemPath);

        Stream IZipFileHandle.Reader => Reader;

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

        protected override string GetNodeDump() => Archive.Path + "+" + ItemPath;
    }
}