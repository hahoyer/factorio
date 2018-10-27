using System;
using System.Collections.Generic;
using System.IO;
using hw.Helper;

namespace Lua
{
    public interface IContext: IDisposable
    {
        object this[string key] {get; set;}
        IEnumerable<string> PackagePath{get; set;}
        object Run(string value);
        object Run(SmbFile value);
        IData FromItem(object value);
        object ToItem(IData value);
    }

    public interface IData
    {
        IDictionary<object, object> TableAsDictionary { get; }
    }

    class Number : IData
    {
        readonly double Value;
        public Number(double value) { Value = value; }
        IDictionary<object, object> IData.TableAsDictionary => null;
    }

}