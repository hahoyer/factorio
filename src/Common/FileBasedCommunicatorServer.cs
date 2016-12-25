using System;
using System.IO;
using hw.DebugFormatter;
using hw.Helper;


namespace Common
{
    public class FileBasedCommunicatorServer
    {
        readonly string Address;
        readonly Func<string, string, string> Get;
        readonly FileSystemWatcher Watcher;

        public FileBasedCommunicatorServer(string address, Func<string, string, string> get)
        {
            Address = address;
            Get = get;
            Watcher = new FileSystemWatcher(address)
            {
                Filter = "*" + Constants.RequestExtension,
                IncludeSubdirectories = true
            };
            Watcher.Renamed += (o, a) => OnFileSeen(a.FullPath);
            Watcher.Created += (o, a) => OnFileSeen(a.FullPath);
        }

        public void Start()
        {
            ("Waiting for requests on " + Address + "\\*" + Constants.RequestExtension).WriteLine();
            Watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            ("Stop waiting for requests on " + Address + "\\*" + Constants.RequestExtension).WriteLine();
            Watcher.EnableRaisingEvents = false;
        }

        void OnFileSeen(string path)
        {
            if(path.EndsWith(Constants.RequestExtension))
                ApplyGettter(path);
        }

        void ApplyGettter(string request)
        {
            var requestFile = request.FileHandle();

            var response = Path.ChangeExtension(request, Constants.ResponseExtension).FileHandle();
            if(response.Exists)
                response.Delete();

            Tracer.Assert(requestFile.DirectoryName.FileHandle().DirectoryName == Address);
            var name = requestFile.DirectoryName.FileHandle().Name;

            var tempResponseFile = requestFile;
            tempResponseFile.String = Get(name, tempResponseFile.String);
            tempResponseFile.Name = response.Name;
        }
    }
}