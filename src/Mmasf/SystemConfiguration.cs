using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSavefiles
{
    sealed class SystemConfiguration
    {
        const string ConfigPathTag = "config-path";

        internal static readonly SystemConfiguration Instance = Create();

        static SystemConfiguration Create()
            => new SystemConfiguration(Configuration.Instance.SystemPath);

        readonly IniFile File;

        SystemConfiguration(string fileName) { File = new IniFile(fileName); }

        internal string CurrentConfigurationPath =>
            File.Global[ConfigPathTag].PathFromFactorioStyle();
    }
}