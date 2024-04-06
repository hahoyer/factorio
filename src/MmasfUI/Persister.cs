using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI;

sealed class Persister : DumpableObject
{
    sealed class Member<T> : DumpableObject, IMember
    {
        public readonly Action<T> Load;
        public readonly Func<T> Store;
        readonly string Name;
        readonly IPersitenceHandler<T> Handler;

        public Member(string name, Action<T> load, Func<T> store, IPersitenceHandler<T> handler)
        {
                Name = name;
                Load = load;
                Store = store;
                Handler = handler;
            }

        void IMember.Load()
        {
                var value = Handler.Get(Name);
                if(value != null)
                    Load(value);
            }

        void IMember.Store() => Handler.Set(Name, Store());
    }

    interface IMember
    {
        void Load();
        void Store();
    }

    readonly IDictionary<string, IMember> Members = new ConcurrentDictionary<string, IMember>();

    readonly SmbFile Handle;

    internal Persister(SmbFile handle) => Handle = handle;

    [EnableDump]
    string FileName => Handle.FullName;

    public void Register<T>(string name, Action<T> load, Func<T> store)
        =>
            Members.Add
            (
                name,
                new Member<T>(name, load, store, new FilePersistenceHandler<T>(FileName)));

    public void Load()
    {
            foreach(var member in Members)
                member.Value.Load();
        }

    public void Store()
    {
            foreach(var member in Members)
                member.Value.Store();
        }

    public void Store(string name) => Members[name].Store();
}