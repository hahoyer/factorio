using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Reader;

namespace ManageModsAndSavefiles
{
    public sealed class MmasfContext : DumpableObject
    {
        public static readonly MmasfContext Instance = new MmasfContext();

        readonly CompoundCache<Configuration> ConfigurationCache;
        readonly CompoundCache<ModConfiguration> ModConfigurationCache;
        readonly CompoundCache<SystemConfiguration> SystemConfigurationCache;
        readonly CompoundCache<DataConfiguration> DataConfigurationCache;
        readonly CompoundCache<UserConfiguration[]> UserConfigurationsCache;
        public readonly FunctionCache<string, FunctionCache<Version, ModDescription>> ModDictionary;

        MmasfContext()
        {
            ConfigurationCache = new CompoundCache<Configuration>(Configuration.Create);
            ModConfigurationCache = new CompoundCache<ModConfiguration>(ModConfiguration.Create);

            ModDictionary = new FunctionCache<string, FunctionCache<Version, ModDescription>>
                (CreateModDescription);

            SystemConfigurationCache = new CompoundCache<SystemConfiguration>
            (
                () => SystemConfiguration.Create(Configuration.SystemPath,"#"),
                ConfigurationCache
            );

            DataConfigurationCache = new CompoundCache<DataConfiguration>
            (
                () =>
                    new DataConfiguration(SystemConfiguration.ConfigurationPath),
                SystemConfigurationCache
            );

            UserConfigurationsCache = new CompoundCache<UserConfiguration[]>
            (
                () =>
                    Configuration
                        .UserConfigurationPaths
                        .Select
                        (
                            item =>
                                UserConfiguration.Create
                                    (item, Configuration.UserConfigurationPaths, this))
                        .ToArray());
        }

        [DisableDump]
        public Configuration Configuration => ConfigurationCache.Value;
        [DisableDump]
        public ModConfiguration ModConfiguration => ModConfigurationCache.Value;
        [DisableDump]
        public SystemConfiguration SystemConfiguration => SystemConfigurationCache.Value;
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

                var executable = Configuration.SystemPath.ToSmbFile()
                    .DirectoryName
                    .PathCombine("bin\\x64\\factorio.exe");

                result += "Executable path: " + executable + "\n";

                var versionInfo = FileVersionInfo.GetVersionInfo(executable);
                var version = versionInfo.FileVersion;

                result += "Version: " + version + "\n";
                return result;
            }
        }

        static FunctionCache<Version, ModDescription> CreateModDescription(string name)
            => new FunctionCache<Version, ModDescription>
                (version => new ModDescription(name, version));

        public void RenewUserConfigurationPaths()
        {
            Configuration.RenewUserConfigurationPaths();
            ConfigurationCache.Invalidate();
            UserConfigurationsCache.Invalidate();
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
            var lookAhead = reader.GetBytes(150);
            var version = new Version
                (reader.GetNext<byte>(), reader.GetNext<byte>(), reader.GetNext<byte>());
            return ModDictionary[name][version];
        }
    }
}