using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSaveFiles.Compression
{
    public interface IZipArchiveHandle
    {
        IEnumerable<IZipFileHandle> Items { get; }
    }
}