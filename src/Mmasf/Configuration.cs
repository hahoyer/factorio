using System;
using System.Collections.Generic;
using System.Linq;
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

        static string GetOriginalUserPath()
            => Extension.SystemWriteDataDir
                .PathCombine(UserConfiguration.ConfigurationDirectoryName)
                .FileHandle()
                .FullName;

        internal static readonly Configuration Instance = Create();

        static Configuration Create()
        {
            var result = Path.FromJsonFile<Configuration>()
                ?? new Configuration();

            if(result.OriginalUserPath == null)
                result.OriginalUserPath = GetOriginalUserPath();

            if(result.SystemPath == null)
                result.SystemPath = SystemConfigurationStatics.Path;

            result.Persist();
            return result;
        }

        public string SystemPath;
        public string OriginalUserPath;

        void Persist()
        {
            Path.FileHandle().EnsureDirectoryOfFileExists();
            Path.ToJsonFile(this);
        }
    }
}