using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Common
{
    public sealed class FileBasedCommunicatorServer : DumpableObject, IDisposable
    {
        readonly string Address;
        readonly Func<string, string, string, string> Get;
        readonly FileSystemWatcher Watcher;

        public FileBasedCommunicatorServer(string uri, Func<string, string, string, string> get)
        {
            Address = Constants.RootPath.PathCombine(uri);
            Address.FileHandle().EnsureIsExistentDirectory();
            Get = get;

            Watcher = new FileSystemWatcher(Address)
            {
                Filter = "*" + Constants.RequestExtension,
                IncludeSubdirectories = true
            };
            Watcher.Renamed += (o, a) => OnFileSeen(a.FullPath);
            Watcher.Created += (o, a) => OnFileSeen(a.FullPath);
        }

        void IDisposable.Dispose() { Stop(); }

        public void Start()
        {
            ("Waiting for requests on " + Address + "\\*" + Constants.RequestExtension).WriteLine();
            Watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            ("Stop waiting for requests on " + Address + "\\*" + Constants.RequestExtension)
                .WriteLine();
            Watcher.EnableRaisingEvents = false;
        }

        void OnFileSeen(string path)
        {
            if(path.EndsWith(Constants.RequestExtension))
                ApplyGetter(path);
        }

        void ApplyGetter(string request)
        {
            var requestFile = request.FileHandle();

            var response = Path.ChangeExtension(request, Constants.ResponseExtension).FileHandle();
            if(response.Exists)
                response.Delete();

            var requestDirectoryFile = requestFile.DirectoryName.FileHandle();
            var methodName = requestDirectoryFile.Name;
            var requestDirectoryRootFile = requestDirectoryFile.DirectoryName.FileHandle();
            Tracer.Assert(requestDirectoryRootFile.DirectoryName == Address);
            var className = requestDirectoryRootFile.Name;

            var tempResponseFile = requestFile;
            tempResponseFile.String = Get(className, methodName, tempResponseFile.String);
            tempResponseFile.Name = response.Name;
        }
    }
}