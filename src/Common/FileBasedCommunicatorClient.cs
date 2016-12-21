using System;
using System.IO;
using System.Threading;
using hw.Helper;


namespace Common
{
    public class FileBasedCommunicatorClient
    {
        readonly string Address;

        readonly Semaphore Semaphore = new Semaphore(0, 1);
        public FileBasedCommunicatorClient(string address) { Address = address; }

        public string Get(string data)
        {
            var guid = Guid.NewGuid();
            var address = Address.PathCombine(guid.ToString());
            var request = address + Constants.RequestExtension;
            var requestFile = request.FileHandle();
            if(requestFile.Exists)
                requestFile.Delete();

            var tempRequest = address + Constants.TempExtension;
            var tempRequestFile = tempRequest.FileHandle();
            tempRequestFile.EnsureDirectoryOfFileExists();
            tempRequestFile.String = data;


            var response = address + Constants.ResponseExtension;
            var responseFile = response.FileHandle();
            var w = new FileSystemWatcher(responseFile.DirectoryName)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                Filter = responseFile.Name
            };

            w.Renamed += (o, a) => OnFileSeen(a.FullPath);
            w.Created += (o, a) => OnFileSeen(a.FullPath);

            tempRequestFile.Name = request;

            Semaphore.WaitOne();

            var result = responseFile.String;

            responseFile.Delete();
            return result;
        }

        void OnFileSeen(string path)
        {
            if(path.EndsWith(Constants.ResponseExtension))
                Semaphore.Release();
        }
    }
}