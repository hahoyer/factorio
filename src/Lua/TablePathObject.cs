using System;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Lua;

abstract class TablePathObject : DumpableObject
{
    sealed class ImbeddedObject : DumpableObject, IObjectView
    {
        readonly object Value;
        public ImbeddedObject(object value) => Value = value;

        object IObjectView.AsObject => Value;
        string IObjectView.AsString => Value.ToString();

        string IObjectView.ToDebugString => Value.ToString();

        object IObjectView.Value => Value;
    }

    sealed class FunctionObject : TablePathObject, IObjectView
    {
        public FunctionObject(HWLua parent, IRootObject rootObject, string[] path)
            : base(parent, rootObject, path) { }

        object IObjectView.AsObject => throw new NotImplementedException();

        string IObjectView.AsString => throw new NotImplementedException();
        string IObjectView.ToDebugString => ToDebugString;
        object IObjectView.Value => throw new NotImplementedException();
    }

    sealed class UserDataObject : TablePathObject, IObjectView
    {
        public UserDataObject(HWLua parent, IRootObject rootObject, string[] path)
            : base(parent, rootObject, path) { }

        object IObjectView.AsObject => throw new NotImplementedException();

        string IObjectView.AsString => throw new NotImplementedException();
        string IObjectView.ToDebugString => ToDebugString;
        object IObjectView.Value => throw new NotImplementedException();
    }

    public readonly HWLua Parent;

    [EnableDump]
    public readonly string[] Path;

    [EnableDump]
    public readonly IRootObject RootObject;

    protected TablePathObject(HWLua parent, IRootObject rootObject, params string[] path)
    {
        Parent = parent;
        RootObject = rootObject;
        Path = path;
    }

    protected override string GetNodeDump()
        => $"{GetType().Name}:{RootObject.ToDebugString}{Path.Select(k => "." + k).Stringify("")}";

    protected IntPtr Kernel => Parent.Kernel;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string ToDebugString => GetNodeDump();

    [DisableDump]
    public LuaTypes LuaType
    {
        get
        {
            Parent.Push(RootObject, Path);

            try
            {
                return (LuaTypes)Kernel.LuaType(-1);
            }
            finally
            {
                Parent.Stack.Drop();
            }
        }
    }

    public IObjectView Materialize()
    {
        Parent.Push(RootObject, Path);

        try
        {
            var type = (LuaTypes)Kernel.LuaType(-1);
            switch(type)
            {
                case LuaTypes.Boolean:
                    return new ImbeddedObject(Kernel.LuaToBoolean(-1));
                case LuaTypes.Number:
                    return new ImbeddedObject(Kernel.LuaNetToNumber(-1));
                case LuaTypes.String:
                    return new ImbeddedObject
                    (
                        ((CharPtr)Kernel
                            .LuaToLString(-1, out var strLen))
                        .ToString((int)strLen)
                    );

                case LuaTypes.Function:
                    return new FunctionObject(Parent, RootObject, Path);
                case LuaTypes.UserData:
                    return new UserDataObject(Parent, RootObject, Path);
                case LuaTypes.Table:
                    return new TableView(Parent, RootObject, Path);
                case LuaTypes.Nil:
                    return null;
                default:
                    NotImplementedMethod();
                    return null;
            }
        }
        finally
        {
            Parent.Stack.Drop();
        }
    }
}