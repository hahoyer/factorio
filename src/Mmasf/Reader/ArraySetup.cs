using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSavefiles.Reader
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ArraySetup : Attribute
    {
        public int Level = 0;
        public readonly Type CountType;
        public readonly int Count;
        public int MaxCount;

        public ArraySetup(int count) { Count = count; }
        public ArraySetup(Type countType) { CountType = countType; }

        public void AssertValid()
        {
            if(CountType == null && Count == 0)
                throw new InvalidException("CountType and Count not set");
            if(CountType != null && Count > 0)
                throw new InvalidException("CountType and Count are both set");
            if(Count < 0)
                throw new InvalidException("Count is negative");
        }

        public sealed class InvalidException : Exception
        {
            public InvalidException(string message)
                : base(message) { }
        }
    }
}