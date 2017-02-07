using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using hw.DebugFormatter;

namespace ManageModsAndSavefiles.Compression.Microsoft
{
    [Obsolete("",true)]
    public sealed class ZipArchiveHandle : DumpableObject, IDisposable
    {
        IEnumerable<ZipFileHandle> ItemsValue;
        ZipArchive ZipArchiveValue;

        internal readonly string Path;

        internal ZipArchiveHandle(string path) { Path = path; }

        internal IEnumerable<ZipFileHandle> Items => ItemsValue ?? (ItemsValue = GetItems());

        IEnumerable<ZipFileHandle> GetItems()
        {
            var zipFile = ZipArchive;
            // ReSharper disable once AccessToDisposedClosure
            var readOnlyCollection = Profiler.Measure(() => zipFile.Entries);
            return readOnlyCollection
                .Select(item => new ZipFileHandle(this, item.FullName))
                .ToArray();
        }

        protected override string GetNodeDump() => Path;

        internal ZipArchiveEntry GetZipArchiveEntry(string itemPath)
            => Profiler.Measure(() => ZipArchive.GetEntry(itemPath));

        ZipArchive ZipArchive
            => ZipArchiveValue ??
                (ZipArchiveValue = Profiler.Measure(() => ZipFile.OpenRead(Path)))
        ;

        void IDisposable.Dispose() => ZipArchiveValue?.Dispose();
    }
}
