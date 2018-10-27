using System;
using System.Diagnostics;
using hw.DebugFormatter;

namespace Lua
{
    sealed class VirtualObjectView : TablePathObject
    {
        public VirtualObjectView(HWLua parent, IRootObject rootObject, params string[] path)
            : base(parent, rootObject, path) {}

        [DisableDump]
        public object Value
        {
            get
            {
                var m = Materialize();
                NotImplementedMethod();
                return null;
            }
        }
    }
}