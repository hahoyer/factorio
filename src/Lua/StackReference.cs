using hw.DebugFormatter;

namespace Lua;

sealed class StackReference : DumpableObject, IRootObject
{
    readonly int Index;

    public StackReference(int index)
    {
        Index = index;
        (Index >= 0).Assert();
    }

    int IRootObject.BeginAccess(HWLua parent)
    {
        (Index < parent.Stack.Count).Assert();
        return Index;
    }

    void IRootObject.EndAccess(HWLua parent, int index) { }

    string IRootObject.ToDebugString => $"Memory({Index})";
}