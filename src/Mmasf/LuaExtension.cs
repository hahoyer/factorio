using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    static class LuaExtension
    {
        public static void Run(this SmbFile root)
        {
            var basePath = root.PathCombine("data\\base\\data.lua");
            var corePath = root.PathCombine("data\\core\\data.lua");

            var lua = Lua.Extension.Instance;

            lua["package.path"] = new[]
                {
                    "data\\core\\lualib",
                    "data\\core",
                    "data\\base",
                    "data"
                }
                .Select(tail => root.PathCombine(tail).FullName + "\\?.lua")
                .Stringify(";");


            try
            {
                var r1 = lua.DoChunk
                (
                    @"
data = {};

require(""dataloader"")
require(""core.data"")
--require(""base.data"")                                      

",
                    "root");

                var result = lua.DoChunk(new StreamReader(corePath.Reader), corePath.Name);
                Tracer.Dump(lua["data"]).WriteLine();

                "S".WriteLine();
            }
            catch(LuaException e) {}
        }


        internal static void Register()
        {
            //Tracer.Dumper.Configuration.Handlers.Add(typeof(LuaTable), (type, o) => ((LuaTable) o).Dump());
        }
    }
}