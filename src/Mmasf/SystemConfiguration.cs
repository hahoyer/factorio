using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class SystemConfiguration
    {
        const string FileNameEnd = "\\Factorio\\config-path.cfg";
        const string ConfigPathTag = "config-path";

        static readonly string SystemReadDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        internal static readonly string ResultSystemPath
            = SystemReadDataDir.FileHandle()
                .Find(FileNameEnd).First()
                .FullName;

        internal static readonly SystemConfiguration Instance = Create();

        static SystemConfiguration Create()
            => new SystemConfiguration(Configuration.Instance.SystemPath);

        readonly IniFile File;

        SystemConfiguration(string fileName) { File = new IniFile(fileName); }

        internal string CurrentConfigurationPath =>
            File.Global[ConfigPathTag].PathFromFactorioStyle();
    }
}