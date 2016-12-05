using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Ionic.Zip;

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
                return BinaryReader.GetNextString((int) Length);
            }
        }

        long Length
        {
            get
            {
                return
                    ZipFile.Read(ArchivePath)
                        .Entries.Single(item => item.FileName == ItemPath)
                        .UncompressedSize;
            }
        }

        public ZipFileHandle GetItem(string itemPath)
            => new ZipFileHandle(ArchivePath, ItemPath?.PathCombine(itemPath) ?? itemPath);

        public IEnumerable<ZipFileHandle> Items
        {
            get
            {
                using(var zipFile = ZipFile.Read(ArchivePath))
                    return zipFile
                        .Entries
                        .Select(item => new ZipFileHandle(ArchivePath, item.FileName));
            }
        }

        public Stream Reader
        {
            get
            {
                using(var zipFile = ZipFile.Read(ArchivePath))
                {
                    var zipEntry = zipFile.Entries.Single(item => item.FileName == ItemPath);
                    var result = new MemoryStream();
                    zipEntry.Extract(result);
                    return result;
                }
            }
        }

        public BinaryRead BinaryReader => new BinaryRead(Reader, Length);
    }
}