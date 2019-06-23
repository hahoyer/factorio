using System.IO;

namespace ManageModsAndSaveFiles.Compression
{
    public interface IZipFileHandle
    {
        string ItemName { get; }
        int Depth { get; }
        string String { get; }
        Stream Reader { get; }
    }
}