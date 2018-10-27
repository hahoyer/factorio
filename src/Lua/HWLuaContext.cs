using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;

namespace Lua
{
    public sealed class HWLuaContext : DumpableObject, IContext
    {
        public static readonly IContext Instance = new HWLuaContext();

        readonly HWLua HWLua = new HWLua();

        object IContext.this[string key] {get => Global[key]; set => Global[key] = value;}

        IEnumerable<string> IContext.PackagePath
        {
            get => ((string) Global["package.path"])?.Split(';');
            set => Global["package.path"] = value.Stringify(";");
        }

        public IDictionary<string, object> Global => HWLua.Global;

        object IContext.Run(string value) => throw new NotImplementedException();

        object IContext.Run(SmbFile value)
        {
            HWLua.Kernel.LuaNetLoadFile(value.FullName);
            return null;
        }

        IData IContext.FromItem(object value) => throw new NotImplementedException();
        object IContext.ToItem(IData value) => throw new NotImplementedException();
    }
}