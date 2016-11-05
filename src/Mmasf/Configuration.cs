using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class Configuration
    {
        const string FileNameEnd = "factorioMmasf\\config.json";
        static readonly string Path
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine(FileNameEnd);

        internal static readonly Configuration Instance = Create();

        static Configuration Create()
        {
            var result = Path.FromJsonFile<Configuration>()
                ?? new Configuration();

            if(result.OriginalUserPath == null)
                result.OriginalUserPath = UserConfiguration.OriginalUserPath;

            if(result.SystemPath == null)
                result.SystemPath = SystemConfiguration.ResultSystemPath;

            result.Persist();
            return result;
        }

        public string SystemPath;
        public string OriginalUserPath;

        void Persist() => Path.ToJsonFile(this);

    }
}