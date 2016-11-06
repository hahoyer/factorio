using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]

namespace ManageModsAndSavefiles
{
    class Program
    {
        static readonly ILog Log = LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Log.Info("Start");
            UserConfiguration.Current.InitializeFrom(UserConfiguration.Original);
            Log.Info("End");
        }
    }
}