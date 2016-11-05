using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class Configuration
    {
        internal static readonly Configuration Instance = Create();

        static Configuration Create()
        {
            var result = Constants.ConfigFileName.FromJsonFile<Configuration>()
                ?? new Configuration();

            if(result.OriginalUserPath == null)
                result.OriginalUserPath = Constants
                    .SystemWriteData
                    .PathCombine("config")
                    .FileHandle()
                    .FullName;

            if(result.SystemPath == null)
                result.SystemPath = Constants.SystemReadData
                    .FileHandle()
                    .Find(Constants.SystemConfigNameEnd)
                    .First()
                    .FullName;

            result.Persist();
            return result;
        }

        public string SystemPath;
        public string OriginalUserPath;

        void Persist() => Constants.ConfigFileName.ToJsonFile(this);

    }
}