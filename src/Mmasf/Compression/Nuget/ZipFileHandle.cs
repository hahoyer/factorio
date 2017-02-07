using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ICSharpCode.SharpZipLib.Zip;
using ManageModsAndSavefiles.Reader;

namespace ManageModsAndSavefiles.Compression.Nuget
{
    public sealed class ZipFileHandle : DumpableObject
    {
        readonly ValueCache<ZipEntry> ZipArchiveEntryCache;
        readonly ZipArchiveHandle Archive;
        readonly string ItemPath;

        internal string ItemName => ItemPath.Split('/').Last();
        internal int Depth => ItemPath.Split('/').Length;

        internal ZipFileHandle(ZipArchiveHandle archive, string itemPath)
        {
            Archive = archive;
            ItemPath = itemPath;
            ZipArchiveEntryCache = new ValueCache<ZipEntry>(GetZipArchiveEntry);
        }

        [DisableDump]
        internal string String
        {
            get
            {
                Tracer.Assert(!string.IsNullOrEmpty(ItemPath));
                return new BinaryRead(Reader).GetNextString((int)Length);
            }
        }

        long Length { get { return Profiler.Measure(() => ZipArchiveEntryCache.Value.Size); } }


        ZipEntry GetZipArchiveEntry()
            => Profiler.Measure(() => Archive.GetZipArchiveEntry(ItemPath));

        internal Stream Reader
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