using hw.DebugFormatter;

namespace Lua {
    internal abstract class NativeStack : DumpableObject
    {
        internal readonly HWLua Parent;

        protected NativeStack(HWLua parent) => Parent = parent;

        protected int Count => Parent.Kernel.LuaGetTop();

        protected VirtualObjectView GetVirtualObject(int index)
            => new VirtualObjectView(Parent, new StackReference(index));

        public void Drop() => Parent.Kernel.LuaSetTop(Count - 1);
        public void Push(string value) => Parent.Kernel.LuaPushString(value);

        public void Push(object value)
        {
            switch(value)
            {
                case string stringValue:
                    Push(stringValue);
                    break;
                default:
                    NotImplementedMethod(value);
                    break;
            }
        }
    }
}