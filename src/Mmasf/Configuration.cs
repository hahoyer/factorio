using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class Configuration
    {
        public class DataClass
        {
            public string SystemPath;
            public string[] UserConfigurationRootPaths;
        }

        const string FileNameEnd = "config.json";
        static readonly string Path = SystemConfiguration.Folder.PathCombine(FileNameEnd);

        readonly DataClass Data;

        readonly ValueCache<string[]> UserConfigurationPathsCache;

        public Configuration()
        {
            Data = Create();
            UserConfigurationPathsCache = new ValueCache<string[]>
            (
                ()
                    => UserConfigurationRootPaths
                        .SelectMany(UserConfiguration.Paths)
                        .ToArray());
            Persist();
        }

        public string SystemPath => Data.SystemPath;
        public string[] UserConfigurationRootPaths => Data.UserConfigurationRootPaths;
        public string[] UserConfigurationPaths => UserConfigurationPathsCache.Value;

        static DataClass Create()
        {
            var result = Path.FromJsonFile<DataClass>() ?? new DataClass();

            if(result.SystemPath == null ||
               !result.SystemPath.ToSmbFile().Exists)
                result.SystemPath = SystemConfiguration.Path;

            if(result.UserConfigurationRootPaths == null)
                result.UserConfigurationRootPaths = new[] {Extension.SystemWriteDataDir};

            return result;
        }

        void Persist()
        {
            Path.ToSmbFile().EnsureDirectoryOfFileExists();
            Path.ToJsonFile(Data);
        }

        public void RenewUserConfigurationPaths() { UserConfigurationPathsCache.IsValid = false; }
    }
}