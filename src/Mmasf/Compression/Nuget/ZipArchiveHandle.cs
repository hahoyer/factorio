using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using ICSharpCode.SharpZipLib.Zip;

namespace ManageModsAndSavefiles.Compression.Nuget
{
    public sealed class ZipArchiveHandle : DumpableObject
    {
        IEnumerable<ZipFileHandle> ItemsValue;
        ZipFile ZipArchiveValue;

        internal readonly string Path;

        internal ZipArchiveHandle(string path) { Path = path; }

        internal IEnumerable<ZipFileHandle> Items => ItemsValue ?? (ItemsValue = GetItems());

        IEnumerable<ZipFileHandle> GetItems()
        {
            return ZipArchive
                .Cast<ZipEntry>()
                .Select(entry => new ZipFileHandle(this, entry.Name))
                .ToArray();
        }

        protected override string GetNodeDump() => Path;

        internal ZipEntry GetZipArchiveEntry(string itemPath)
            => ZipArchive.GetEntry(itemPath);

        internal ZipFile ZipArchive => ZipArchiveValue ?? (ZipArchiveValue = GetZipArchive());

        ZipFile GetZipArchive() => new ZipFile(File.OpenRead(Path));
    }
}