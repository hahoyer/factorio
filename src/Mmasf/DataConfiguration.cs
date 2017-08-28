using System;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class DataConfiguration
    {
        const string PathSectionName = "path";
        const string WriteDataTag = "write-data";
        const string ConfigurationIniFileName = "config.ini";

        readonly IniFile IniFile;

        internal DataConfiguration(string fileName, Action onExternalModification)
        {
            var path = fileName.PathCombine(ConfigurationIniFileName);
            IniFile = new IniFile(path, commentString: ";", onExternalModification: onExternalModification);
            RootUserConfigurationPath = fileName.ToSmbFile().DirectoryName;
        }

        public string CurrentUserConfigurationPath
        {
            get => FactorioStyleCurrentUserConfigurationPath.PathFromFactorioStyle();
            set => FactorioStyleCurrentUserConfigurationPath = value;
        }

        string FactorioStyleCurrentUserConfigurationPath
        {
            get => IniFile[PathSectionName][WriteDataTag];
            set
            {
                IniFile[PathSectionName][WriteDataTag] = value;
                IniFile.Persist();
            }
        }

        public string RootUserConfigurationPath { get; }
    }
}