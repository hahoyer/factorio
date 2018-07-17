namespace Lua
{
    public static class Extension
    {
        public static readonly IContext Instance = new MoonLuaContext();
    }

    public class MoonLuaContext: DumpableObject, IContext{}
}