using System.Linq;
using hw.Helper;

namespace ManageModsAndSaveFiles
{
    public sealed class Configuration
    {
        sealed class DataClass
        {
            public string[] Exceptions;
            public string SystemPath;
            public string[] UserConfigurationRootPaths;
        }

        static readonly SmbFile Path = SystemConfiguration
            .GetProgramFolder()
            .PathCombine(Constants.FileNameEnd);

        SmbFile[] GetUserConfigurationPaths(SmbFile root)
            => root
                .RecursiveItems()
                .Where(file=>!file.ContainsAnyPattern(root, Exceptions))
                .Where(IsRelevantPathCandidate)
                .ToArray();

        static bool IsRelevantPathCandidate(SmbFile item)
            =>
                item.IsDirectory &&
                IsExistent(item, Constants.SaveDirectoryName, true) &&
                IsExistent(item, Constants.ModDirectoryName, true);

        static bool IsExistent(SmbFile item, string fileName, bool isDictionary)
        {
            var fileHandle = item.FullName.PathCombine(fileName).ToSmbFile();
            return fileHandle.Exists && fileHandle.IsDirectory == isDictionary;
        }

        internal readonly string[] UserConfigurationRootPaths;
        internal readonly string[] Exceptions;
        readonly string SystemPath;

        readonly ValueCache<SmbFile[]> UserConfigurationPathsCache;

        public Configuration()
        {
            var jsonFile = Path.FullName.FromJsonFile<DataClass>();
            SystemPath = jsonFile?.SystemPath;
            UserConfigurationRootPaths = jsonFile?.UserConfigurationRootPaths;
            Exceptions = jsonFile?.Exceptions;

            var systemPath = SystemPath?.ToSmbFile();
            if(systemPath?.Exists != true)
                SystemPath = MmasfContext
                    .Instance
                    .SystemConfiguration
                    .Path
                    .FullName;

            if(UserConfigurationRootPaths == null)
                UserConfigurationRootPaths = new[] {Extension.SystemWriteDataDir.FullName};

            if(Exceptions == null)
                Exceptions = new string[0];

            UserConfigurationPathsCache = new ValueCache<SmbFile[]>
            (
                ()
                    => UserConfigurationRootPaths
                        .Select(x => x.ToSmbFile())
                        .SelectMany(GetUserConfigurationPaths)
                        .ToArray());
            Persist();
        }

        public SmbFile SystemFile => SystemPath.ToSmbFile();

        public SmbFile[] UserConfigurationPaths => UserConfigurationPathsCache.Value;

        void Persist()
        {
            Path.EnsureDirectoryOfFileExists();
            Path.FullName.ToJsonFile
            (
                new DataClass
                {
                    SystemPath = SystemPath,
                    UserConfigurationRootPaths = UserConfigurationRootPaths, 
                    Exceptions = Exceptions
                }
            );
        }

        public void RenewUserConfigurationPaths() {UserConfigurationPathsCache.IsValid = false;}
    }
}