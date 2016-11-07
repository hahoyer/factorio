using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSavefiles
{
    sealed class Context
    {
        internal static Context Instance = new Context();

        internal readonly Configuration Configuration;
        internal readonly SystemConfiguration SystemConfiguration;
        internal readonly DataConfiguration DataConfiguration;

        Context()
        {
            Configuration = Configuration.Create();
            SystemConfiguration = new SystemConfiguration(Configuration.SystemPath);
            DataConfiguration = new DataConfiguration(SystemConfiguration.ConfigurationPath);
        }
    }
}