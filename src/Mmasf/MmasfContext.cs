﻿using System;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSaveFiles.Mods;
using ManageModsAndSaveFiles.Reader;

namespace ManageModsAndSaveFiles;

public sealed class MmasfContext : DumpableObject
{
    public static readonly MmasfContext Instance = new();

    public readonly FunctionCache<string, FunctionCache<Version, ModDescription>> ModDictionary;

    public Action OnExternalModification;
    public Action OnModificationOnConfigPaths;

    readonly CompoundCache<Configuration> ConfigurationCache;
    readonly CompoundCache<DirectoryWatcher> ConfigurationWatcherCache;
    readonly CompoundCache<DataConfiguration> DataConfigurationCache;
    readonly CompoundCache<ModConfiguration> ModConfigurationCache;
    readonly CompoundCache<SystemConfigurationFile> SystemConfigurationFileCache;
    readonly CompoundCache<UserConfiguration[]> UserConfigurationsCache;
    readonly CompoundCache<SystemConfiguration> SystemConfigurationCache;

    [DisableDump]
    public SystemConfiguration SystemConfiguration => SystemConfigurationCache.Value;

    [DisableDump]
    public Configuration Configuration => ConfigurationCache.Value;

    [DisableDump]
    public ModConfiguration ModConfiguration => ModConfigurationCache.Value;

    [DisableDump]
    public SystemConfigurationFile SystemConfigurationFile => SystemConfigurationFileCache.Value;

    [DisableDump]
    public DataConfiguration DataConfiguration => DataConfigurationCache.Value;

    [DisableDump]
    public UserConfiguration[] UserConfigurations => UserConfigurationsCache.Value;

    [DisableDump]
    public string FactorioInformation
    {
        get
        {
            var result = "";

            var executable = Configuration.SystemFile
                .DirectoryName
                .PathCombine(@"bin\x64\factorio.exe");

            result += "Executable path: " + executable + "\n";

            var versionInfo = FileVersionInfo.GetVersionInfo(executable);
            var version = versionInfo.FileVersion;

            result += "Version: " + version + "\n";
            return result;
        }
    }

    MmasfContext()
    {
        ConfigurationCache = new(() => new(), ConfigurationWatcherCache);
        ConfigurationWatcherCache = new(CreateConfigurationWatcher);
        ModConfigurationCache = new(ModConfiguration.Create);
        ModDictionary = new(CreateModDescription);
        SystemConfigurationCache = new(() => new());

        SystemConfigurationFileCache = new(
            () => new(Configuration.SystemFile),
            ConfigurationCache
        );

        DataConfigurationCache = new(
            () => new(SystemConfigurationFile.ConfigurationPath, OnExternalModification),
            SystemConfigurationFileCache
        );

        UserConfigurationsCache = new(
            () =>
                Configuration
                    .UserConfigurationPaths
                    .Select
                    (
                        item =>
                            UserConfiguration
                                .Create(item, Configuration.UserConfigurationPaths, this)
                    )
                    .ToArray()
        );
    }

    static FunctionCache<Version, ModDescription> CreateModDescription(string name)
        => new(version => new(name, version));

    DirectoryWatcher CreateConfigurationWatcher()
        => new(Configuration.UserConfigurationRootPaths, Configuration.Exceptions)
        {
            OnExternalModification = OnModificationOnConfigPaths
        };

    public void RenewUserConfigurationPaths()
    {
        Configuration.RenewUserConfigurationPaths();
        ConfigurationCache.IsValid = false;
        UserConfigurationsCache.IsValid = false;
    }

    internal ModDescription CreateModReferenceBefore014(int i, BinaryRead reader)
    {
        var name = reader.GetNextString<int>();

        var version = new Version
            (reader.GetNext<short>(), reader.GetNext<short>(), reader.GetNext<short>());
        return ModDictionary[name][version];
    }

    public ModDescription CreateModReference(int i, BinaryRead reader, bool isBefore01414)
    {
        if(isBefore01414)
            return CreateModReferenceBefore014(i, reader);

        var name = reader.GetNextString<byte>();
        // ReSharper disable once UnusedVariable
        var lookAhead = reader.LookAhead();
        var version = new Version
            (reader.GetNext<byte>(), reader.GetNext<byte>(), reader.GetNext<byte>());
        return ModDictionary[name][version];
    }

    public void ActivateWatcher() => ConfigurationWatcherCache.IsValid = true;
}