using System;
using System.Runtime.Remoting.Channels;
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

            ChannelServices.RegisterChannel(new FileBasedClientChannel(directory), false);
            var data = (ITestData) Activator.GetObject(typeof(ITestData), "");


            "Response: ".WriteLine();
            data.TestString.WriteLine();

            "(End)Press any key:".WriteLine();
            var k = Console.ReadKey();
        }
    }
}