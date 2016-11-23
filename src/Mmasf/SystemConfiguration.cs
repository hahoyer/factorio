using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Helper;
using log4net;

namespace ManageModsAndSavefiles
{
    public sealed class SystemConfiguration
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

        internal static SystemConfiguration Create(string fileName)
            => new SystemConfiguration(fileName);

        readonly IniFile File;

        SystemConfiguration(string fileName) { File = new IniFile(fileName); }

        internal string ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();

    }
}