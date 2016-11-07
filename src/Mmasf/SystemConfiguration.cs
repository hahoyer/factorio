using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Helper;
using log4net;

namespace ManageModsAndSavefiles
{
    sealed class SystemConfiguration
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        const string FileNameEnd = "Factorio\\config-path.cfg";
        const string ConfigPathTag = "config-path";

        static readonly string SystemReadDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        internal static string Path
            => Log.Value
            (
                "Path",
                () => SystemReadDataDir
                    .FileHandle()
                    .FindFilesThatEndsWith(FileNameEnd).First()
                    .FullName
            );

        readonly IniFile File;

        internal SystemConfiguration(string fileName) { File = new IniFile(fileName); }

        internal string ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();
    }
}