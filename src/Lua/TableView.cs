using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Lua
{
    sealed class TableView : TablePathObject, IDictionary<string, object>, IObjectView
    {
        sealed class Enumerator : IEnumerator<KeyValuePair<string, object>>
        {
            readonly TableView Parent;
            string CurrentKey;

            public Enumerator(TableView parent) => Parent = parent;

            void IDisposable.Dispose() {}

            bool IEnumerator.MoveNext() {return (CurrentKey = Parent.Parent.GetNextKey(Parent.RootObject, CurrentKey, Parent.Path))!= null;}

            void IEnumerator.Reset() {CurrentKey = null;}

            KeyValuePair<string, object> IEnumerator<KeyValuePair<string, object>>.Current
                => new KeyValuePair<string, object>(CurrentKey, Parent[CurrentKey]);

            object IEnumerator.Current => CurrentKey;
        }

        public TableView(HWLua parent, IRootObject rootObject, params string[] path)
            : base(parent, rootObject, path) {}

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Clear() {throw new NotImplementedException();}

        bool ICollection<KeyValuePair<string, object>>.Contains
            (KeyValuePair<string, object> item) => throw new NotImplementedException();

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            var index = arrayIndex;
            foreach(var key in Keys)
            {
                array[index] = new KeyValuePair<string, object>(key, this[key].Materialize());
                index++;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove
            (KeyValuePair<string, object> item) => throw new NotImplementedException();

        int ICollection<KeyValuePair<string, object>>.Count => Keys.Count();

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        bool IDictionary<string, object>.ContainsKey(string key) => throw new NotImplementedException();
        void IDictionary<string, object>.Add(string key, object value) {throw new NotImplementedException();}
        bool IDictionary<string, object>.Remove(string key) => throw new NotImplementedException();

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
            => throw new NotImplementedException();

        object IDictionary<string, object>.this[string key]
        {
            get => this[key.Split('.')].Value;
            set => Set(key.Split('.'), value);
        }

        ICollection<string> IDictionary<string, object>.Keys => Keys.ToArray();

        ICollection<object> IDictionary<string, object>.Values 
            => Keys.Select(key=>this[key].Materialize()).ToArray();

        string IObjectView.AsString
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        object IObjectView.AsObject
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        string IObjectView.ToDebugString => ToDebugString;


        object IObjectView.Value
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        IEnumerable<string> Keys => Parent.GetKeys(RootObject, Path);

        public VirtualObjectView this[params string[] key]
            => new VirtualObjectView(Parent, RootObject, Path.Concat(key).ToArray());

        void Set(string[] path, object value)
            => Parent.Set(RootObject, value, Path.Concat(path).ToArray());
    }
}