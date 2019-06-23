using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace ManageModsAndSaveFiles
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
            => new[] { SteamPath, SystemReadDataDir }
                .Where(f => f != null)
                .FindFilesThatEndsWith(FileNameEnd)
                .First();

        SmbFile GetExecutablePath() => Path
            .DirectoryName
            .ToSmbFile()
            .FindFilesThatEndsWith(ExecutableName)
            .Single();

        SmbFile ExecutablePathCache;
        SmbFile PathCache;

        public SmbFile ExecutablePath
            => ExecutablePathCache ?? (ExecutablePathCache = GetExecutablePath());
    }
    public sealed class SystemConfigurationFile : DumpableObject
    {
        const string ConfigPathTag = "config-path";

        readonly IniFile File;

        internal SystemConfigurationFile(SmbFile fileName)
        {
            Tracer.Assert
                (fileName.Exists, "System configuration file not found: " + fileName);
            File = new IniFile(fileName, "#", OnExternalModification);
        }

        public SmbFile ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();

        static void OnExternalModification() { throw new NotImplementedException(); }

    }
}