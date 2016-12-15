using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ManageModsAndSavefiles.Reader
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class Ignore : Attribute, BinaryRead.IAdvancer
    {
        readonly int Count;
        readonly int LineNumber;

        public Ignore(int count, [CallerLineNumber] int lineNumber = 0)
        {
            Count = count;
            LineNumber = lineNumber;
        }

        int BinaryRead.IAdvancer.Value => LineNumber;

        public void Execute(BinaryRead reader) { reader.Position += Count; }
    }
}