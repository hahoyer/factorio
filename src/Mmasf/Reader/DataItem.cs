using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ManageModsAndSaveFiles.Reader
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DataItem : Attribute, BinaryRead.IAdvancer
    {
        public object CaptureIdentifier;
        public Type Reader;
        internal readonly string FileName;
        internal readonly int LineNumber;


        public DataItem
        (
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            FileName = fileName;
            LineNumber = lineNumber;
        }

        int BinaryRead.IAdvancer.Value => LineNumber;
    }
}