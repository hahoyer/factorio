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

            ChannelServices.RegisterChannel(new FileBasedClientChannel(), false);
            var data = (ITestData) Activator.GetObject(typeof(ITestData), "filebased://localhost/Mmasf");


            "Response to TestString: ".WriteLine();
            data.TestString.WriteLine();

            "Response to TestFunction: ".WriteLine();
            data.TestFunction(12,"lorum").WriteLine();

            "(End)Press any key:".WriteLine();
            var k = Console.ReadKey();
        }
    }
}