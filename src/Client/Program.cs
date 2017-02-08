using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using hw.DebugFormatter;

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

            var client = new FileBasedClient("Mmasf");

            var data = client.Get<ManageModsAndSavefiles>();

            "Response to FactorioInformation: ".WriteLine();
            data.FactorioInformation.WriteLine();

            "Response to TestFunction: ".WriteLine();
            data.UserConfigurations[0].Path.WriteLine();

            "(End)Press any key:".WriteLine();
            var k = Console.ReadKey();
        }
    }
}