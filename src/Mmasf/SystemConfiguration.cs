using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class SystemConfiguration
    {
        const string FileNameEnd = "Factorio\\config-path.cfg";
        const string ConfigPathTag = "config-path";

        static readonly string SystemReadDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        internal static string Path
            => SystemReadDataDir
                .FileHandle()
                .FindFilesThatEndsWith(FileNameEnd).First()
                .FullName
            ;

        internal static SystemConfiguration Create(string fileName)
            => new SystemConfiguration(fileName);

        readonly IniFile File;

        SystemConfiguration(string fileName) { File = new IniFile(fileName); }

        public string ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();
    }
}