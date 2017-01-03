using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using hw.DebugFormatter;
using hw.Helper;

namespace Common
{
    public sealed class FileBasedCommunicatorClient : DumpableObject
    {
        readonly string Address;

        readonly Semaphore Semaphore = new Semaphore(0, 1);
        public FileBasedCommunicatorClient(string address) { Address = address; }

        public string Get(string[] data, string identifier)
        {
            var address = Address.PathCombine
            (
                Process.GetCurrentProcess().Id.ToString(),
                Thread.CurrentThread.ManagedThreadId.ToString(),
                identifier
            );

            var request = address + Constants.RequestExtension;
            var requestFile = request.FileHandle();
            if(requestFile.Exists)
                requestFile.Delete();

            var tempRequest = address + Constants.TempExtension;
            var tempRequestFile = tempRequest.FileHandle();
            tempRequestFile.EnsureDirectoryOfFileExists();
            tempRequestFile.String = data.Select(Extension.EscapeComma).Stringify(",");

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

            ("Waiting for response on " + response).WriteLine();
            Semaphore.WaitOne();

            var result = responseFile.String;

            responseFile.Delete();
            return result;
        }

        public string Get(string identifier)
        {
            NotImplementedMethod(identifier);
            return null;
        }

        void OnFileSeen(string path)
        {
            if(path.EndsWith(Constants.ResponseExtension))
                Semaphore.Release();
        }
    }
}