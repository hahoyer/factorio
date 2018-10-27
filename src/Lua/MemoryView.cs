using hw.DebugFormatter;

namespace Lua {
    internal sealed class MemoryView : NativeStack
    {
        public readonly StackView Top;

        public MemoryView(HWLua parent)
            : base(parent) => Top = new StackView(parent);

        public new int Count => base.Count;

        public VirtualObjectView this[int index]
        {
            get
            {
                Tracer.Assert(index >= 0);
                Tracer.Assert(index < Count);
                return GetVirtualObject(index);
            }
        }

        internal void GetTable(string key, int index)
        {
            Tracer.Assert(index >= 0);
            Tracer.Assert(index < Count);

            Parent.Stack.Push(key);
            Parent.Kernel.LuaGetTable(index + 1);
        }
    }
}