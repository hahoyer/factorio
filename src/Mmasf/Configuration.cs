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
        static readonly SmbFile Path = SystemConfiguration.Folder.FullName.PathCombine(FileNameEnd).ToSmbFile();

        static DataClass Create()
        {
            var result = Path.FullName.FromJsonFile<DataClass>() ?? new DataClass();

            var systemPath = result.SystemPath?.ToSmbFile();
            if(systemPath?.Exists != true)
                result.SystemPath = SystemConfiguration.Path.FullName;

            if(result.UserConfigurationRootPaths == null)
                result.UserConfigurationRootPaths = new[] {Extension.SystemWriteDataDir.FullName};

            return result;
        }

        readonly DataClass Data;

        readonly ValueCache<SmbFile[]> UserConfigurationPathsCache;

        public Configuration()
        {
            Data = Create();
            UserConfigurationPathsCache = new ValueCache<SmbFile[]>
            (
                ()
                    => UserConfigurationRootPaths
                        .SelectMany(UserConfiguration.Paths)
                        .ToArray());
            Persist();
        }

        public SmbFile SystemPath => Data.SystemPath.ToSmbFile();

        public SmbFile[] UserConfigurationRootPaths => Data.UserConfigurationRootPaths.Select
                (x => x.ToSmbFile())
            .ToArray();

        public SmbFile[] UserConfigurationPaths => UserConfigurationPathsCache.Value;

        void Persist()
        {
            Path.EnsureDirectoryOfFileExists();
            Path.FullName.ToJsonFile(Data);
        }

        public void RenewUserConfigurationPaths() {UserConfigurationPathsCache.IsValid = false;}
    }
}