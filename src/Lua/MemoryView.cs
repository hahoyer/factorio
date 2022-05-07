using hw.DebugFormatter;

namespace Lua;

sealed class MemoryView : NativeStack
{
    public readonly StackView Top;

    public MemoryView(HWLua parent)
        : base(parent) => Top = new(parent);

    public new int Count => base.Count;

    public VirtualObjectView this[int index]
    {
        get
        {
            (index >= 0).Assert();
            (index < Count).Assert();
            return GetVirtualObject(index);
        }
    }

    internal void GetTable(string key, int index)
    {
        (index >= 0).Assert();
        (index < Count).Assert();

        Parent.Stack.Push(key);
        Parent.Kernel.LuaGetTable(index + 1);
    }
}