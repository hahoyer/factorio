using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class SystemConfiguration : DumpableObject
    {
        const string ConfigPathTag = "config-path";
        const string ExecutableName = "factorio.exe";
        const string FileNameEnd = "Factorio\\config-path.cfg";
        const string ProgramFolderName = "FactorioMmasf";
        const string SteamPathInRegistry = @"HKEY_CURRENT_USER\Software\Valve\Steam\SteamPath";

        static readonly SmbFile SystemReadDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                .ToSmbFile();

        static readonly SmbFile SteamPath
            = SteamPathInRegistry
                .Registry()
                .GetValue<string>()
                .ToSmbFile();

        public static readonly SmbFile Folder = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .PathCombine(ProgramFolderName)
            .ToSmbFile();


        internal static SmbFile Path
            => new[] {SteamPath, SystemReadDataDir}
                .Where(f => f != null)
                .FindFilesThatEndsWith(FileNameEnd)
                .First();

        internal static SystemConfiguration Create(SmbFile fileName, string commentString)
            => new SystemConfiguration(fileName, commentString);

        static void OnExternalModification() {throw new NotImplementedException();}

        public static SmbFile ExecutablePath
            => Path
                .DirectoryName
                .ToSmbFile()
                .FindFilesThatEndsWith(ExecutableName)
                .Single();

        readonly IniFile File;

        SystemConfiguration(SmbFile fileName, string commentString)
        {
            Tracer.Assert
                (fileName.Exists, "System configuration file not found: " + fileName);
            File = new IniFile(fileName, commentString, OnExternalModification);
        }

        public SmbFile ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();
    }
}