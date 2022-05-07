using System;

namespace ManageModsAndSaveFiles.Reader;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public sealed class ArrayItem : Attribute
{
    public sealed class InvalidException : Exception
    {
        public InvalidException(string message)
            : base(message) { }
    }

    public readonly int Count;
    public int Level = 0;
    public Type CountType;
    public int MaxCount;
    public Type Reader;

    public ArrayItem(int count = 0) => Count = count;

    public void AssertValid()
    {
        if(CountType != null && Count > 0)
            throw new InvalidException("CountType and Count are both set");
        if(Count < 0)
            throw new InvalidException("Count is negative");
    }
}