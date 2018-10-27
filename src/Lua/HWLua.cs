using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

namespace Lua
{
    sealed class HWLua
    {
        static readonly IRootObject GlobalRootObject = new GlobalRootObject();

        static MemoryView StackInstance;
        public static int StaticStackLength => StackInstance.Count;

        [DisableDump]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly MemoryView Stack;

        internal readonly TableView Global;

        internal readonly IntPtr Kernel;

        public HWLua()
        {
            Kernel = NativeMethods.LuaLNewState();
            Stack = new MemoryView(this);
            Global = new TableView(this, GlobalRootObject);
            Kernel.LuaLOpenLibs();

            StackInstance = Stack;
        }

        [UsedImplicitly]
        public Array MemoryForDebug
            => Stack
                .Count
                .Select(index => Stack[index].ToDebugString)
                .ToArray();

        public void Set(IRootObject rootObject, object value, params string[] path)
        {
            if(path.Length < 1)
                throw new Exception("key required");

            Push(rootObject,path.Take(path.Length - 1).ToArray());

            Stack.Push(path.Last());
            Stack.Push(value);
            Kernel.LuaSetTable(-3);
            Stack.Drop();
        }

        public void Push(IRootObject rootObject, params string[] path)
        {
            var index = rootObject.BeginAccess(this);
            Kernel.LuaPushValue(index+1);
            foreach(var key in path)
            {
                Stack.GetTable(key, 0);
                Kernel.LuaRemove(-2);
            }
            rootObject.EndAccess(this, index);
        }

        public string GetNextKey(IRootObject rootObject, string currentKey, params string[] path)
        {
            Push(rootObject, path);
            Stack.Push(currentKey);
            var result = Kernel.LuaNext(-2) != 0;
            Stack.Drop();
            var newKey = Stack.Top[0].Materialize().AsString;
            Stack.Drop();
            Stack.Drop();
            return result? newKey:null;
        }

        public IEnumerable<string> GetKeys(IRootObject rootObject, params string[] path)
        {
            Push(rootObject, path);
            Kernel.LuaPushNil();
            var result = new List<string>();
            while(Kernel.LuaNext(-2) != 0)
            {
                Stack.Drop();
                var key = Stack.Top[0].Materialize().AsString;
                result.Add(key);
            }

            Stack.Drop();
            Stack.Drop();
            return result.ToArray();
        }
    }
}