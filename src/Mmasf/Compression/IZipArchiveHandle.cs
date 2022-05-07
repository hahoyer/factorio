using System.Collections.Generic;

namespace ManageModsAndSaveFiles.Compression;

public interface IZipArchiveHandle
{
    IEnumerable<IZipFileHandle> Items { get; }
}