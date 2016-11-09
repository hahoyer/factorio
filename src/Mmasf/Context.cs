using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSavefiles
{
    sealed class Context
    {
        internal static readonly Context Instance = new Context();

        internal readonly Configuration Configuration;
        internal readonly SystemConfiguration SystemConfiguration;
        internal readonly DataConfiguration DataConfiguration;
        internal readonly UserConfiguration[] UserConfigurations;
        ModMatrix ModMatrix;

        Context()
        {
            Configuration = Configuration.Create();
            SystemConfiguration = new SystemConfiguration(Configuration.SystemPath);
            DataConfiguration = new DataConfiguration(SystemConfiguration.ConfigurationPath);
            UserConfigurations = Configuration
                .UserConfigurationPaths
                .Select(UserConfiguration.Create)
                .ToArray();
            ModMatrix = new ModMatrix(Configuration.UserConfigurationPaths);
        }
    }
}