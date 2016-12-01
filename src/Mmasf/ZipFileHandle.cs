using System;
using System.Collections.Generic;
using System.IO;
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
                Tracer.Assert(!string.IsNullOrEmpty(ItemPath));

                var zipArchive = ZipFile
                    .Open(ArchivePath, ZipArchiveMode.Read);
                var entry = zipArchive.GetEntry(ItemPath);

                using(var s = entry.Open())
                {
                    var buffer = new byte[MaxLength];
                    var actualLength = s.Read(buffer, 0, MaxLength);
                    Tracer.Assert(actualLength < MaxLength);
                    return Encoding.UTF8.GetString(buffer, 0, actualLength);
                }
            }
        }

        public ZipFileHandle GetItem(string itemPath)
            => new ZipFileHandle(ArchivePath, ItemPath?.PathCombine(itemPath) ?? itemPath);
    }

    public enum ZipArchiveMode
    {
        Read
    }

    public static class ZipFile
    {
        public static ZipArchive Open(string path, ZipArchiveMode mode)
            => new ZipArchive(path, mode);

        public static Stream OpenStream(string path, ZipArchiveMode mode, string entryName)
        {
            //var t = typeof(System.IO.Packaging.Package).Assembly
            //    .GetType("MS.Internal.IO.Zip.ZipArchive");
            return null;
        }
    }

    public sealed class ZipArchive
    {
        readonly string Path;
        readonly ZipArchiveMode Mode;

        public ZipArchive(string path, ZipArchiveMode mode)
        {
            Path = path;
            Mode = mode;
        }

        public ZipArchiveEntry GetEntry(string entryName) => new ZipArchiveEntry(this, entryName);

        public Stream OpenStream(string entryName) => ZipFile.OpenStream(Path, Mode, entryName);
    }

    public sealed class ZipArchiveEntry
    {
        readonly ZipArchive Parent;
        readonly string EntryName;

        public ZipArchiveEntry(ZipArchive parent, string entryName)
        {
            Parent = parent;
            EntryName = entryName;
        }

        public Stream Open() => Parent.OpenStream(EntryName);
    }
}