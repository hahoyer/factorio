using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using Neo.IronLua;


namespace Lua
{
    public sealed class NeoLuaContext : DumpableObject, IContext
    {
        public static IContext Instance => new NeoLuaContext();

        readonly LuaGlobalPortable Data;

        NeoLuaContext() { Data = new Neo.IronLua.Lua().CreateEnvironment(); }

        object IContext.this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        IEnumerable<string> IContext.PackagePath
        {
            get => ((string) Data["package.path"])?.Replace("/", "\\").Split(';');
            set => Data["package"] = new LuaTable {["path"] = value.Stringify(";").Replace("\\", "/")};
        }

        object IContext.Run(string value) => FromItem(Data.DoChunk(value, "root"));
        object IContext.Run(SmbFile value) => throw new NotImplementedException();
        IData IContext.FromItem(object value) { return FromItem(value); }
        object IContext.ToItem(IData value) { return ToItem(value); }

        object ToItem(IData value)
        {
            NotImplementedMethod(value);
            return null;
        }

        IData FromItem(object value)
        {
            NotImplementedMethod(value);
            return null;
        }
    }
}