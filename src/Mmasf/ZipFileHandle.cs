using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class ZipFileHandle : DumpableObject
    {
        readonly string ArchivePath;
        readonly string ItemPath;

        public string ItemName => ItemPath.Split('/').Last();

        public ZipFileHandle(string archivePath, string itemPath = null)
        {
            ArchivePath = archivePath;
            ItemPath = itemPath;
        }

        [DisableDump]
        public string String
        {
            get
            {
                Tracer.Assert(!string.IsNullOrEmpty(ItemPath));
                return new BinaryRead(Reader).GetNextString((int) Length);
            }
        }

        long Length
        {
            get
            {
                return
                    ZipFile.OpenRead(ArchivePath)
                        .Entries.Single(item => item.Name == ItemPath)
                        .Length;
            }
        }

        public ZipFileHandle GetItem(string itemPath)
            => new ZipFileHandle(ArchivePath, ItemPath?.PathCombine(itemPath) ?? itemPath);

        public IEnumerable<ZipFileHandle> Items
        {
            get
            {
                using(var zipFile = ZipFile.OpenRead(ArchivePath))
                {
                    var readOnlyCollection = zipFile.Entries;
                    return readOnlyCollection
                        .Select(item => new ZipFileHandle(ArchivePath, item.Name));
                }
            }
        }

        public Stream Reader
        {
            get
            {
                var zipArchive = ZipFile.OpenRead(ArchivePath);
                var zipEntry = zipArchive.Entries.Single(item => item.Name == ItemPath);
                var zipReader = zipEntry.Open();
                var reader = new SeekableReader(zipReader, Length);

                return new StreamWithCleanupList(reader, reader, zipReader, zipArchive);
            }
        }
    }
}