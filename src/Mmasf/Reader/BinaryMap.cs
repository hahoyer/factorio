using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSaveFiles.Reader
{
    public sealed class BinaryMap<TTarget> : DumpableObject
    {
        long[] SizesCache;

        long Offset(int fieldIndex)
        {
            NotImplementedMethod(fieldIndex);
            return 0;
        }

    }
}