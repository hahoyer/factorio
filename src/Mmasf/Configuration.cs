using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class Configuration
    {
        const string FileNameEnd = "FactorioMmasf\\config.json";

        static readonly string Path
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine(FileNameEnd);

        internal static Configuration Create()
        {
            var result = Path.FromJsonFile<Configuration>()
                ?? new Configuration();

            if (result.SystemPath == null)
                result.SystemPath = SystemConfiguration.Path;

            if (result.UserConfigurationPaths == null)
                result.UserConfigurationPaths = UserConfiguration.Paths;

            result.Persist();
            return result;
        }

        public string SystemPath;
        public string[] UserConfigurationPaths;

        void Persist()
        {
            Path.FileHandle().EnsureDirectoryOfFileExists();
            Path.ToJsonFile(this);
        }

        internal void RenewUserConfigurationPaths()
        {
            UserConfigurationPaths = UserConfiguration.Paths;
            Persist();
        }
    }
}