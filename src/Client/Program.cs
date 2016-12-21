using System;
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

            var directory = Constants.RootPath.PathCombine("Mmasf");

            var response = new FileBasedCommunicatorClient(directory).Get("Test Text");
            "Response: ".WriteLine();
            response.WriteLine();

            "(End)Press any key:".WriteLine();
            var k = Console.ReadKey();
        }
    }
}