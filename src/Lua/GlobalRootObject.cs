using hw.DebugFormatter;

namespace Lua {
    sealed class GlobalRootObject : DumpableObject, IRootObject
    {
        int IRootObject.BeginAccess(HWLua parent)
        {
            parent.Kernel.LuaNetPushGlobalTable();
            return parent.Stack.Count - 1;
        }

        void IRootObject.EndAccess(HWLua parent, int index) 
            => parent.Kernel.LuaRemove(index+1);
        string IRootObject.ToDebugString => "Global";
    }
}