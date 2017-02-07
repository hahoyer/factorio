using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Reader;

namespace ManageModsAndSavefiles.Compression.Microsoft
{
    [Obsolete("", true)]
    public sealed class ZipFileHandle : DumpableObject
    {
        readonly ValueCache<ZipArchiveEntry> ZipArchiveEntryCache;
        readonly ZipArchiveHandle Archive;
        readonly string ItemPath;

        internal string ItemName => ItemPath.Split('/').Last();
        internal int Depth => ItemPath.Split('/').Length;

        internal ZipFileHandle(ZipArchiveHandle archive, string itemPath)
        {
            Archive = archive;
            ItemPath = itemPath;
            ZipArchiveEntryCache = new ValueCache<ZipArchiveEntry>(GetZipArchiveEntry);
        }

        [DisableDump]
        internal string String
        {
            get
            {
                Tracer.Assert(!string.IsNullOrEmpty(ItemPath));
                return new BinaryRead(Reader).GetNextString((int) Length);
            }
        }

        long Length { get { return Profiler.Measure(() => ZipArchiveEntryCache.Value.Length); } }


        ZipArchiveEntry GetZipArchiveEntry()
            => Profiler.Measure(() => Archive.GetZipArchiveEntry(ItemPath));

        internal Stream Reader
        {
            get
            {
                var zipEntry = ZipArchiveEntryCache.Value;
                var zipReader = zipEntry.Open();
                var reader = new SeekableReader(zipReader, Length);

                return new StreamWithCleanupList(reader, reader, zipReader);
            }
        }

        protected override string GetNodeDump() => Archive.Path + "+" + ItemPath;
    }
}