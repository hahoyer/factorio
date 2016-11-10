using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class ZipFileHandle : DumpableObject
    {
        const int MaxLength = 1024 * 1024;

        readonly string ArchivePath;
        readonly string ItemPath;

        public ZipFileHandle(string archivePath, string itemPath = null)
        {
            ArchivePath = archivePath;
            ItemPath = itemPath;
        }

        public string String
        {
            get
            {
                Tracer.Assert(!string.IsNullOrWhiteSpace(ItemPath));

                var zipArchive = ZipFile
                    .Open(ArchivePath, ZipArchiveMode.Read);
                var entry = zipArchive
                    .GetEntry(ItemPath);

                using(var s = entry.Open())
                {
                    var buffer = new byte[MaxLength];
                    var actualLength = s.Read(buffer, 0, MaxLength);
                    Tracer.Assert(actualLength < MaxLength);
                    return Encoding.UTF8.GetString(buffer,0,actualLength);
                }
            }
        }

        public ZipFileHandle GetItem(string itemPath)
            => new ZipFileHandle(ArchivePath, ItemPath?.PathCombine(itemPath) ?? itemPath);
    }
}