using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace ManageModsAndSaveFiles;

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

    SmbFile ExecutablePathCache;
    SmbFile PathCache;

    internal SmbFile Path => PathCache ?? (PathCache = GetPath());

    public SmbFile ExecutablePath
        => ExecutablePathCache ?? (ExecutablePathCache = GetExecutablePath());

    internal static SmbFile GetProgramFolder() => Environment
        .GetFolderPath(Environment.SpecialFolder.ApplicationData)
        .PathCombine(ProgramFolderName)
        .ToSmbFile();

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
}

public sealed class SystemConfigurationFile : DumpableObject
{
    const string ConfigPathTag = "config-path";

    readonly IniFile File;

    internal SystemConfigurationFile(SmbFile fileName)
    {
        fileName.Exists.Assert
            ("System configuration file not found: " + fileName);
        File = new(fileName, "#", OnExternalModification);
    }

    public SmbFile ConfigurationPath => File.Global[ConfigPathTag].PathFromFactorioStyle();

    static void OnExternalModification() => throw new NotImplementedException();
}