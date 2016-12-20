using System;
using System.IO;
using Common;
using hw.DebugFormatter;
using hw.Helper;


namespace Client
{
    static class Program
    {
        static void Main(string[] args)
        {
            var console = Console.Out;
            Tracer.LinePart("");
            Console.SetOut(console);
            DebugTextWriter.Register(false);

            1.Seconds().Sleep();
            var connection = "TestConnection";

            var directory = Environment
                .GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                .PathCombine("hw.FileCommunicator", "Mmasf", connection);

            var name = "TestRequest";
            var request = name + ".request";
            var fileHandle = directory.PathCombine(request).FileHandle();
            if(fileHandle.Exists)
                fileHandle.Delete();

            var tempRequest = name + ".temp";
            var f = directory.PathCombine(tempRequest).FileHandle();
            "(Start)Press any key:".WriteLine();
            var k = Console.ReadKey();
            f.EnsureDirectoryOfFileExists();
            f.String = "Test Text";


           var w = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                Filter = name + "*.response"
            };

            w.Renamed += (o, a) => OnRenamed(a);

            f.Name = request;

            "(End)Press any key:".WriteLine();
            k = Console.ReadKey();
        }

        static void OnRenamed(RenamedEventArgs args)
        {
            var response = args.FullPath.FileHandle();
            "Response:".WriteLine();
            response.String.WriteLine();
        }
    }
}