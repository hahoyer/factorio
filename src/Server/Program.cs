using System;
using System.Collections.Generic;
using System.Linq;
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
            RemotingConfiguration.RegisterWellKnownServiceType
                (typeof(ManageModsAndSavefiles), "", WellKnownObjectMode.Singleton);

            using(var server = new FileBasedServer("Mmasf"))
            {
                var instance = new ManageModsAndSavefiles
                {
                    FactorioInformation = "FactorioInformation Test",
                    UserConfigurations = new[]
                    {
                        new UserConfiguration
                        {
                            Path = "Test path"
                        }
                    }
                };
                server.Register(instance);
                "(Server)Press any key:".WriteLine();
                var k = Console.ReadKey();
            }
        }
    }
}