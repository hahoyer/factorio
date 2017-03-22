using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class SystemConfiguration
    {
        const string FileNameEnd = "Factorio\\config-path.cfg";
        const string ConfigPathTag = "config-path";
        const string SteamPathInRegistry = @"HKEY_CURRENT_USER\Software\Valve\Steam\SteamPath";

        static readonly string SystemReadDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        static readonly string SteamPath
            = SteamPathInRegistry
                .Registry()
                .GetValue<string>();

        internal static string Path
            => new[] {SteamPath, SystemReadDataDir}
                .Where(f => f != null)
                .Select(f => f.ToSmbFile())
                .FindFilesThatEndsWith(FileNameEnd).First()
                .FullName;

        internal static SystemConfiguration Create(string fileName)
            => new SystemConfiguration(fileName);

        readonly IniFile File;

        SystemConfiguration(string fileName)
        {
            Tracer.Assert
                (fileName.ToSmbFile().Exists, "System configuration file not found: " + fileName);
            File = new IniFile(fileName);
        }

        public string ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();
        const string ProgramFolderName = "FactorioMmasf";
        public static readonly string Folder = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .PathCombine(ProgramFolderName);
    }
}