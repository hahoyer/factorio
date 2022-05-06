using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Lua;

public sealed class MoonLuaContext : DumpableObject, IContext
{
    [UnitTest]
    public sealed class Tester
    {
        [UnitTest]
        [Test]
        public static void Test()
        {
            var x = Instance.Run(@"return tonumber(""0x23"")");
            (x != null).Assert();
        }
    }

    readonly Script Script;

    MoonLuaContext() => Script = new();

    IData IContext.FromItem(object value) => FromItem(value);

    object IContext.this[string key]
    {
        get => Script.Globals[key];
        set => Script.Globals[key] = value;
    }

    IEnumerable<string> IContext.PackagePath
    {
        get => ((ScriptLoaderBase)Script.Options.ScriptLoader).ModulePaths;
        set => ((ScriptLoaderBase)Script.Options.ScriptLoader).ModulePaths = value.ToArray();
    }

    object IContext.Run(string value) => FromItem(Script.DoString(value));
    object IContext.Run(SmbFile value) => FromItem(Script.DoFile(value.FullName));
    object IContext.ToItem(IData value) => ToItem(value);

    public static IContext Instance => new MoonLuaContext();

    static object ToItem(IData value) => throw new NotImplementedException();

    IData FromItem(object value)
    {
        if(value is DynValue item)
        {
            switch(item.Type)
            {
                case DataType.Nil:
                case DataType.Void:
                    return null;
                case DataType.Boolean:
                    NotImplementedMethod(item);
                    return null;
                case DataType.Number:
                    return new Number(item.Number);
                case DataType.String:
                    NotImplementedMethod(item);
                    return null;
                case DataType.Function:
                    NotImplementedMethod(item);
                    return null;
                case DataType.Table:
                    NotImplementedMethod(item);
                    return null;
                case DataType.Tuple:
                    NotImplementedMethod(item);
                    return null;
                case DataType.UserData:
                    NotImplementedMethod(item);
                    return null;
                case DataType.Thread:
                    NotImplementedMethod(item);
                    return null;
                case DataType.ClrFunction:
                    NotImplementedMethod(item);
                    return null;
                case DataType.TailCallRequest:
                    NotImplementedMethod(item);
                    return null;
                case DataType.YieldRequest:
                    NotImplementedMethod(item);
                    return null;
            }

            NotImplementedMethod(item);
            return null;
        }

        NotImplementedMethod(value);
        return null;
    }
}