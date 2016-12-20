using System;
using System.IO;
using Common;
using hw.DebugFormatter;
using hw.Helper;


namespace Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            var console = Console.Out;
            Tracer.LinePart("");
            Console.SetOut(console);
            DebugTextWriter.Register(false);

            var connection = "TestConnection";

            var directory = Environment
                .GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                .PathCombine("hw.FileCommunicator", "Mmasf", connection);

            var request = "TestRequest";

            var f = directory.PathCombine(request).FileHandle();
            var w = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                Filter = "*.request"
            };

            w.Renamed += (o, a) => OnRenamed(a);

            "(Server)Press any key:".WriteLine();

            var k = Console.ReadKey();
        }

        static void OnRenamed(RenamedEventArgs args)
        {
            ($"{args.ChangeType}\n" +
             $"\tFullPath = {args.FullPath}\n" +
             $"\tName = {args.Name}\n" +
             $"\tOldName = {args.OldName}")
                .WriteLine();

            if(Path.GetExtension(args.Name) != ".request")
                return;
            var response = Path.ChangeExtension(args.FullPath, "response").FileHandle();
            if(response.Exists)
                response.Delete();

            var tempResponse =args.FullPath.FileHandle();
            tempResponse.String = "response to " + tempResponse.String;
            tempResponse.Name = response.Name;

        }

        static void OnChanged(FileSystemEventArgs args)
        {
            return;
            ($"{args.ChangeType}\n" +
             $"\tFile = {args.FullPath}\n" +
             $"\tName = {args.Name}")
                .WriteLine();

            switch(args.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    break;
                default:
                    return;
            }

            var text = args.FullPath;

            text.WriteLine();
        }
    }
}