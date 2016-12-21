using System;
using System.IO;
using hw.Helper;


namespace Common
{
    public class FileBasedCommunicatorServer
    {
        readonly Func<string, string> Get;
        readonly FileSystemWatcher Watcher;

        public FileBasedCommunicatorServer(string address, Func<string, string> get)
        {
            Get = get;
            Watcher = new FileSystemWatcher(address)
            {
                Filter = "*" + Constants.RequestExtension
            };
            Watcher.Renamed += (o, a) => OnFileSeen(a.FullPath);
            Watcher.Created += (o, a) => OnFileSeen(a.FullPath);
        }

        public void Start() { Watcher.EnableRaisingEvents = true; }

        public void Stop() { Watcher.EnableRaisingEvents = false; }

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

            var tempResponseFile = requestFile;
            tempResponseFile.String = Get(tempResponseFile.String);
            tempResponseFile.Name = response.Name;
        }
    }
}