using System.IO;

namespace ManageModsAndSavefiles.Compression
{
    public interface IZipFileHandle
    {
        string ItemName { get; }
        int Depth { get; }
        string String { get; }
        Stream Reader { get; }
    }
}