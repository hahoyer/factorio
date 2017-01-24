using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class DataConfiguration
    {
        const string PathSectionName = "path";
        const string WriteDataTag = "write-data";
        const string ConfigurationIniFileName = "config.ini";

        readonly IniFile IniFile;

        internal DataConfiguration(string fileName)
        {
            IniFile = new IniFile(fileName.PathCombine(ConfigurationIniFileName));
            RootUserConfigurationPath = fileName.FileHandle().DirectoryName;
        }

        public string CurrentUserConfigurationPath
        {
            get { return FactorioStyleCurrentUserConfigurationPath.PathFromFactorioStyle(); }
            set { FactorioStyleCurrentUserConfigurationPath = value.PathToFactorioStyle(); }
        }

        string FactorioStyleCurrentUserConfigurationPath
        {
            get { return IniFile[PathSectionName][WriteDataTag]; }
            set
            {
                IniFile[PathSectionName][WriteDataTag] = value;
                IniFile.Persist();
            }
        }

        public string RootUserConfigurationPath { get; }
    }
}