using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles;

namespace Net2Assembly
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