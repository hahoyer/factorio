using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSavefiles.Compression
{
    public interface IZipArchiveHandle
    {
        IEnumerable<IZipFileHandle> Items { get; }
    }
}