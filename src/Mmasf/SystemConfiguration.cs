using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

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

    // When incorporating this class into SystemConfiguration, strange instantiation occurs
    static class SystemConfigurationStatics
    {
        const string FileNameEnd = "Factorio\\config-path.cfg";

        static readonly string SystemReadDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        internal static string Path
            => SystemReadDataDir
                .FileHandle()
                .FindFilesThatEndsWith(FileNameEnd).First()
                .FullName;
    }
}