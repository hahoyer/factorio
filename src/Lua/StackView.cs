using hw.DebugFormatter;

namespace Lua {
    internal sealed class StackView : NativeStack
    {
        public StackView(HWLua parent)
            : base(parent) {}

        public new int Count => base.Count;

        public VirtualObjectView this[int index]
        {
            get
            {
                Tracer.Assert(index >= 0);
                Tracer.Assert(index < Count);
                return GetVirtualObject(Count - index - 1);
            }
        }
    }
}