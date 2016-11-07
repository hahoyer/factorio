using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]

namespace ManageModsAndSavefiles
{
    static class Program
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Log.Info("Start");
            var i = Context.Instance.DataConfiguration.Path;
            Log.Info("End");
        }
    }
}