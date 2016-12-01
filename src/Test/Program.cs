using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles;

namespace Test
{
    class Program
    {
        public static void Main(string[] args)
        {
            var context = MmasfContext.Instance;
            Tracer.Line(context.FactorioInformation);
            Tracer.Line(context.SystemConfiguration.ConfigurationPath);
            Tracer.Line(context.UserConfigurations.Select(item => item.Path).Stringify("\n"));
        }
    }
}