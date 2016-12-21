using System;
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

            var directory = Constants.RootPath.PathCombine("Mmasf");

            var server = new FileBasedCommunicatorServer(directory, a => "response to " + a);
            server.Start();
            "(Server)Press any key:".WriteLine();

            var k = Console.ReadKey();
        }
    }
}