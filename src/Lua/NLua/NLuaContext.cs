using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using NLua;
using NLua.Exceptions;
using NLua_Lua = NLua.Lua;

namespace Lua.NLua;

public sealed class NLuaContext : DumpableObject, IContext
{
    sealed class SimpleValue : IData
    {
        readonly object Value;
        public SimpleValue(object value) => Value = value;
        IDictionary<object, object> IData.TableAsDictionary => null;
    }

    sealed class Table : IData
    {
        readonly LuaTable Value;
        public Table(LuaTable value) => Value = value;
        public IDictionary<object, object> TableAsDictionary => Value.GetTableAsDictionary();
    }

    static NLuaContext() => Tracer.Dumper
        .Configuration
        .Handlers
        .Add(typeof(LuaTable), (type, o) => Extension.Dump((LuaTable)o));


    readonly NLua_Lua Data;


    NLuaContext() => Data = new();

    IData IContext.FromItem(object value)
    {
        switch(value)
        {
            case LuaTable luaTable:
                return new Table(luaTable);
            case bool valueBool:
                return new SimpleValue(valueBool);
            case double valueDouble:
                return new SimpleValue(valueDouble);
            case string valueString:
                return new SimpleValue(valueString);
            case LuaFunction valueFuction:
                return new SimpleValue(valueFuction);
        }


        NotImplementedMethod(value);
        return null;
    }

    object IContext.this[string key]
    {
        get => Data[key];
        set => Data[key] = value;
    }

    IEnumerable<string> IContext.PackagePath
    {
        get => ((string)Data["package.path"])?.Split(';');
        set => Data["package.path"] = value.Stringify(";");
    }

    object IContext.Run(string value) => ExceptionGuard(() => Data.DoString(value));
    object IContext.Run(SmbFile value) => ExceptionGuard(() => Data.DoFile(value.FullName));

    object IContext.ToItem(IData value) => throw new NotImplementedException();

    public static IContext Instance => new NLuaContext();

    public IDictionary<object, object> DataDictionary => throw new NotImplementedException();

    TResult ExceptionGuard<TResult>(Func<TResult> function)
    {
        try
        {
            return function();
        }
        catch(LuaException e)
        {
            e.Message.ParseExceptionMessage(this).Log();
            return default;
        }
    }
}