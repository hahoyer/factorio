using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.Win32;

namespace ManageModsAndSavefiles
{
    public sealed class SystemConfiguration
    {
        const string FileNameEnd = "Factorio\\config-path.cfg";
        const string ConfigPathTag = "config-path";

        static readonly string SystemReadDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        static string SteamPath
        {
            get
            {
                using(var steam = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam"))
                    return (string) steam?.GetValue("SteamPath");
            }
        }

        internal static string Path
            => new[] {SteamPath, SystemReadDataDir}
                .Select(f => f.FileHandle())
                .FindFilesThatEndsWith(FileNameEnd).First()
                .FullName;

        internal static SystemConfiguration Create(string fileName)
            => new SystemConfiguration(fileName);

        readonly IniFile File;

        SystemConfiguration(string fileName)
        {
            Tracer.Assert
                (fileName.FileHandle().Exists, "System configuration file not found: " + fileName);
            File = new IniFile(fileName);
        }

        public string ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();
    }
}