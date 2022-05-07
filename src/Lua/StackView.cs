using hw.DebugFormatter;

namespace Lua;

sealed class StackView : NativeStack
{
    public StackView(HWLua parent)
        : base(parent) { }

    public new int Count => base.Count;

    public VirtualObjectView this[int index]
    {
        get
        {
            (index >= 0).Assert();
            (index < Count).Assert();
            return GetVirtualObject(Count - index - 1);
        }
    }
}