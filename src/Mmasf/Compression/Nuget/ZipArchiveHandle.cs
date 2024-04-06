using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using ICSharpCode.SharpZipLib.Zip;

namespace ManageModsAndSaveFiles.Compression.Nuget;

public sealed class ZipArchiveHandle : DumpableObject, IZipArchiveHandle

{
    internal readonly string Path;
    IEnumerable<IZipFileHandle> ItemsValue;
    ZipFile ZipArchiveValue;

    internal ZipArchiveHandle(string path) => Path = path;

    IEnumerable<IZipFileHandle> IZipArchiveHandle.Items => ItemsValue ??= GetItems();

    protected override string GetNodeDump() => Path;

    internal ZipFile ZipArchive => ZipArchiveValue ??= GetZipArchive();

    IEnumerable<IZipFileHandle> GetItems() => ZipArchive
        .Cast<ZipEntry>()
        .Select(entry => new ZipFileHandle(this, entry.Name))
        .ToArray();

    internal ZipEntry GetZipArchiveEntry(string itemPath)
        => ZipArchive.GetEntry(itemPath);

    ZipFile GetZipArchive() => new(File.OpenRead(Path));
}