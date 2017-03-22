using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using hw.DebugFormatter;
// ReSharper disable once RedundantUsingDirective
using ManageModsAndSavefiles.Compression.Nuget;

namespace ManageModsAndSavefiles.Compression.Microsoft
{
    public sealed class ZipArchiveHandle : DumpableObject, IDisposable, IZipArchiveHandle
    {
        IEnumerable<IZipFileHandle> ItemsValue;
        ZipArchive ZipArchiveValue;

        internal readonly string Path;

        internal ZipArchiveHandle(string path) { Path = path; }

        IEnumerable<IZipFileHandle> IZipArchiveHandle.Items => ItemsValue ?? (ItemsValue = GetItems());

        IEnumerable<IZipFileHandle> GetItems()
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
