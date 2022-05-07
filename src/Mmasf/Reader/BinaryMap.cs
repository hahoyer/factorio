using hw.DebugFormatter;

namespace ManageModsAndSaveFiles.Reader;

public sealed class BinaryMap<TTarget> : DumpableObject
{
    long[] SizesCache;

    long Offset(int fieldIndex)
    {
        NotImplementedMethod(fieldIndex);
        return 0;
    }
}