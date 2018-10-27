using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

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

        public readonly SmbFile ProgramFolder = GetProgramFolder();

        internal static SmbFile GetProgramFolder()
        {
            return Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine(ProgramFolderName)
                .ToSmbFile();
        }

        internal SmbFile Path => PathCache ?? (PathCache = GetPath());

        static SmbFile GetPath()
            => new[] {SteamPath, SystemReadDataDir}
                .Where(f => f != null)
                .FindFilesThatEndsWith(FileNameEnd)
                .First();

        internal static SystemConfiguration Create(SmbFile fileName, string commentString)
            => new SystemConfiguration(fileName, commentString);

        static void OnExternalModification() {throw new NotImplementedException();}

        SmbFile GetExecutablePath() => Path
            .DirectoryName
            .ToSmbFile()
            .FindFilesThatEndsWith(ExecutableName)
            .Single();

        readonly IniFile File;

        SmbFile ExecutablePathCache;
        SmbFile PathCache;

        SystemConfiguration(SmbFile fileName, string commentString)
        {
            Tracer.Assert
                (fileName.Exists, "System configuration file not found: " + fileName);
            File = new IniFile(fileName, commentString, OnExternalModification);
        }

        public SmbFile ExecutablePath
            => ExecutablePathCache ?? (ExecutablePathCache = GetExecutablePath());

        public SmbFile ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();
    }
}