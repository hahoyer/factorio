using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ManageModsAndSaveFiles.Reader
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class Ignore : Attribute, BinaryRead.IAdvancer
    {
        readonly int Count;
        readonly int LineNumber;
        public object CaptureIdentifier;

        public Ignore(int count, [CallerLineNumber] int lineNumber = 0)
        {
            Count = count;
            LineNumber = lineNumber;
        }

        int BinaryRead.IAdvancer.Value => LineNumber;

        internal void Execute(BinaryRead reader, MemberInfo member)
        {
            reader.Position += Count;
            reader.SignalToUserContext(CaptureIdentifier, member, null);
        }
    }
}