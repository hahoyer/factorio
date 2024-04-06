using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI;

abstract class FilePersistenceHandler : DumpableObject
{
    [EnableDump]
    readonly string FileName;

    protected FilePersistenceHandler(string fileName) => FileName = fileName;

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

    SmbFile FileHandle(string name)
    {
            var result = FileName.PathCombine(name).ToSmbFile();
            result.EnsureDirectoryOfFileExists();
            return result;
        }

    protected void Set(string name, object value) => FileHandle(name).String = value.ToString();
}

sealed class FilePersistenceHandler<T> : FilePersistenceHandler, IPersitenceHandler<T>
{
    public FilePersistenceHandler(string fileName)
        : base(fileName) { }

    T IPersitenceHandler<T>.Get(string name) => (T)Get(typeof(T), name);

    void IPersitenceHandler<T>.Set(string name, T value) => Set(name, value);
}