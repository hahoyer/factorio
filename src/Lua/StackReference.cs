using hw.DebugFormatter;

namespace Lua {
    sealed class StackReference : DumpableObject, IRootObject
    {
        readonly int Index;

        public StackReference(int index)
        {
            Index = index;
            Tracer.Assert(Index >= 0);
        }

        string IRootObject.ToDebugString => $"Memory({Index})";

        int IRootObject.BeginAccess(HWLua parent)
        {
            Tracer.Assert(Index < parent.Stack.Count);
            return Index;
        }

        void IRootObject.EndAccess(HWLua parent, int index) {}
    }
}