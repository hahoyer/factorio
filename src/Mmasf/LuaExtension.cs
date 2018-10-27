using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;
using Lua;
using ManageModsAndSavefiles.Reader;


namespace ManageModsAndSavefiles
{
    static class LuaExtension
    {
        public static void Run(this SmbFile root)
        {
            using(var lua = Lua.Extension.Instance)
            {
                lua.PackagePath = new[]
                    {
                        "data\\core\\lualib",
                        "data\\core",
                        "data\\base",
                        "data"
                    }
                    .Select(tail => root.PathCombine(tail).FullName + "\\?.lua");

                var result = lua.Run("GameLoader.lua".ToSmbFile());


                var value = lua["data"];
                var data
                    = lua.FromItem(value).TableAsDictionary;
                var x = Find(data, "electronic-circuit", lua).ToArray();
                Tracer.Dump(lua["data.raw.recipe"]).WriteLine();
                Tracer.Dump(lua.FromItem(lua["data.raw"]).TableAsDictionary.Keys).WriteLine();
                //Tracer.Dump(result).WriteLine();
            }
        }

        static IEnumerable<string> Find(IDictionary<object, object> data, string target, IContext lua)
        {
            return data.SelectMany(p => Find(p, target, lua));
        }

        static IEnumerable<string> Find(KeyValuePair<object, object> data, string target, IContext lua)
        {
            if(data.Key is string key && key.Contains(target))
                yield return key;

            var deeperData = lua.FromItem(data.Value).TableAsDictionary;
            if(deeperData == null)
                yield break;

            foreach(var searchResult in Find(deeperData, target, lua))
                yield return data.Key + "." + searchResult;
        }
    }
}