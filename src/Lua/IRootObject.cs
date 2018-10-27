namespace Lua
{
    interface IRootObject
    {
        string ToDebugString {get;}
        int BeginAccess(HWLua parent);
        void EndAccess(HWLua parent, int index);
    }
}