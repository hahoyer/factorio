using System;
using System.Runtime.Remoting;
using Common;
using hw.DebugFormatter;


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
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(TestData), "1", WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(TestData1), "2", WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(TestData2), "3", WellKnownObjectMode.Singleton);

            using(var server = new FileBasedServer("Mmasf"))
            {
                "(Server)Press any key:".WriteLine();
                var k = Console.ReadKey();
            }
        }
    }
}