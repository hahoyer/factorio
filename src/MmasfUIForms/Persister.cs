using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUIForms
{
    sealed class Persister : DumpableObject
    {
        sealed class Member<T> : DumpableObject, IMember
        {
            readonly string Name;
            public readonly Action<T> Load;
            public readonly Func<T> Store;
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

        readonly hw.Helper.File Handle;
        [EnableDump]
        string FileName => Handle.FullName;

        internal Persister(hw.Helper.File handle) { Handle = handle; }

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

    interface IPersitenceHandler<T>
    {
        T Get(string name);
        void Set(string name, T value);
    }

    sealed class FilePersistenceHandler<T> : FilePersistenceHandler, IPersitenceHandler<T>
    {
        public FilePersistenceHandler(string fileName)
            : base(fileName) {}

        T IPersitenceHandler<T>.Get(string name) => (T) Get(typeof(T), name);

        void IPersitenceHandler<T>.Set(string name, T value) => Set(name, value);
    }

    abstract class FilePersistenceHandler : DumpableObject
    {
        [EnableDump]
        readonly string FileName;

        protected FilePersistenceHandler(string fileName) { FileName = fileName; }

        protected object Get(Type type, string name)
        {
            var text = FileHandle(name).String;
            if(text == null)
                return null;

            if(type == typeof(Tuple<int, int>))
            {
                var values = text
                    .Substring(1, text.Length - 2)
                    .Split(',')
                    .Take(2)
                    .Select(int.Parse)
                    .ToArray();
                return new Tuple<int, int>(values[0], values[1]);
            }

            NotImplementedMethod(type, name);
            return null;
        }

        hw.Helper.File FileHandle(string name)
        {
            var result = FileName.PathCombine(name).FileHandle();
            result.EnsureDirectoryOfFileExists();
            return result;
        }

        protected void Set(string name, object value) => FileHandle(name).String = value.ToString();
    }
}                                                              