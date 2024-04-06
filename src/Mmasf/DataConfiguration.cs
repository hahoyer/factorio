using System;
using hw.Helper;

namespace ManageModsAndSaveFiles;

public sealed class DataConfiguration
{
    const string PathSectionName = "path";
    const string WriteDataTag = "write-data";
    const string ConfigurationIniFileName = "config.ini";

    readonly IniFile IniFile;

    internal DataConfiguration(SmbFile fileName, Action onExternalModification)
    {
        var path = fileName.PathCombine(ConfigurationIniFileName);
        IniFile = new IniFile(path, commentString: ";", onExternalModification: onExternalModification);
        RootUserConfigurationPath = fileName.DirectoryName;
    }

    public SmbFile CurrentUserConfigurationPath
    {
        get => FactorioStyleCurrentUserConfigurationPath.PathFromFactorioStyle();
        set => FactorioStyleCurrentUserConfigurationPath = value.FullName;
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