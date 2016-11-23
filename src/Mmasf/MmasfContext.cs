using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class MmasfContext
    {
        public static readonly MmasfContext Instance = new MmasfContext();

        readonly CacheInNet<Configuration> ConfigurationCache;
        readonly CacheInNet<SystemConfiguration> SystemConfigurationCache;
        readonly CacheInNet<DataConfiguration> DataConfigurationCache;
        readonly CacheInNet<UserConfiguration[]> UserConfigurationsCache;
        readonly CacheInNet<ModMatrix> ModMatrixCache;

        MmasfContext()
        {
            ConfigurationCache = new CacheInNet<Configuration>(Configuration.Create);

            SystemConfigurationCache = new CacheInNet<SystemConfiguration>
            (
                () => SystemConfiguration.Create(Configuration.SystemPath),
                ConfigurationCache
            );

            DataConfigurationCache = new CacheInNet<DataConfiguration>
            (
                () =>
                    new DataConfiguration(SystemConfiguration.ConfigurationPath),
                SystemConfigurationCache
            );

            UserConfigurationsCache = new CacheInNet<UserConfiguration[]>
            (
                () => Configuration
                    .UserConfigurationPaths
                    .Select
                    (item => UserConfiguration.Create(item, Configuration.UserConfigurationPaths))
                    .ToArray(),
                UserConfigurationsCache
            );

            ModMatrixCache = new CacheInNet<ModMatrix>
                (() => new ModMatrix(UserConfigurations), UserConfigurationsCache);
        }

        public Configuration Configuration => ConfigurationCache.Value;
        public SystemConfiguration SystemConfiguration => SystemConfigurationCache.Value;
        public DataConfiguration DataConfiguration => DataConfigurationCache.Value;
        public UserConfiguration[] UserConfigurations => UserConfigurationsCache.Value;
        public ModMatrix ModMatrix => ModMatrixCache.Value;

        public string FactorioInformation
        {
            get
            {
                var result = "";

                var executable = Configuration.SystemPath
                    .FileHandle().DirectoryName
                    .PathCombine("bin\\x64\\factorio.exe");

                result += "Executable path: " + executable + "\n";

                var versionInfo = FileVersionInfo.GetVersionInfo(executable);
                var version = versionInfo.FileVersion;

                result += "Version: " + version + "\n";
                return result;
            }
        }

        public void RenewUserConfigurationPaths()
        {
            Configuration.RenewUserConfigurationPaths();
            ConfigurationCache.Invalidate();
        }
    }

    abstract class CacheInNet : DumpableObject
    {
        readonly CacheInNet[] DependsOn;

        protected CacheInNet(CacheInNet[] dependsOn) { DependsOn = dependsOn; }

        protected CacheInNet[] AllDependers
        {
            get
            {
                var result = DependsOn;
                while(true)
                {
                    var newResult = result
                        .SelectMany(item => item.DependsOn)
                        .Concat(result)
                        .Distinct()
                        .ToArray();
                    if(result.Length == newResult.Length)
                        return newResult;

                    result = newResult;
                }
            }
        }

        public abstract void Invalidate();
    }

    sealed class CacheInNet<TValue> : CacheInNet
    {
        readonly ValueCache<TValue> Cache;

        public CacheInNet(Func<TValue> getValue, params CacheInNet[] dependsOn)
            : base(dependsOn) { Cache = new ValueCache<TValue>(getValue); }

        public TValue Value => Cache.Value;

        public void OnChange()
        {
            foreach(var item in AllDependers)
                item.Invalidate();
        }

        public override void Invalidate() { Cache.IsValid = false; }
    }
}