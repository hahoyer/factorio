using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Saves;

namespace ManageModsAndSavefiles
{
    public sealed class UserConfiguration : DumpableObject
    {
        const string SaveDirectoryName = "saves";
        internal const string ModDirectoryName = "mods";
        const string PlayerDataFileName = "player-data.json";
        const string ReadDataTag = "read-data";
        const string ModConfigurationFileName = "mod-list.json";

        internal static string[] Paths
            => Extension
                .SystemWriteDataDir.ToSmbFile()
                .RecursiveItems()
                .Where(IsRelevantPathCandidate)
                .Select(item => item.FullName)
                .ToArray();

        static bool IsRelevantPathCandidate(SmbFile item)
            =>
            item.IsDirectory
            && IsExistent(item, PlayerDataFileName, false)
            && IsExistent(item, SaveDirectoryName, true)
            && IsExistent(item, ModDirectoryName, true);

        static bool IsExistent(SmbFile item, string fileName, bool isDictionary)
        {
            var fileHandle = item.FullName.PathCombine(fileName).ToSmbFile();
            return fileHandle.Exists && fileHandle.IsDirectory == isDictionary;
        }

        internal static UserConfiguration Create
            (string item, string[] allPaths, MmasfContext parent)
            => new UserConfiguration(item, allPaths, parent);

        public readonly string Path;
        readonly string[] AllPaths;
        readonly ValueCache<Saves.FileCluster[]> SaveFilesCache;
        readonly ValueCache<Mods.FileCluster[]> ModFilesCache;
        readonly ValueCache<IDictionary<string, bool>> ModConfigurationCache;
        readonly MmasfContext Parent;

        public string Name => Path.Split('\\').Last();
        public bool IsRoot => Path == Parent.DataConfiguration.RootUserConfigurationPath;
        public bool IsCurrent => Path == Parent.DataConfiguration.CurrentUserConfigurationPath;

        UserConfiguration(string path, string[] allPaths, MmasfContext parent)
        {
            Path = path;
            AllPaths = allPaths;
            Parent = parent;
            ModConfigurationCache = new ValueCache<IDictionary<string, bool>>(GetModConfiguration);
            ModFilesCache = new ValueCache<Mods.FileCluster[]>(GetModFiles);
            SaveFilesCache = new ValueCache<Saves.FileCluster[]>(GetSaveFiles);


            var watcher = new FileSystemWatcher(path)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                Filter = "*.request"
            };

            watcher.Renamed += (o, a) => OnRenamed(a);



        }

        string FilesPath(string item) => Path.PathCombine(item);

        Saves.FileCluster[] GetSaveFiles()
        {
            var fileHandle = FilesPath(SaveDirectoryName).ToSmbFile();
            if(!fileHandle.Exists)
                return new Saves.FileCluster[0];

            return fileHandle
                .Items
                .Where(item => !item.IsDirectory && item.Extension.ToLower() == ".zip")
                .Select(item => new Saves.FileCluster(item.FullName, this))
                .ToArray();
        }

        Mods.FileCluster[] GetModFiles()
        {
            var fileHandle = FilesPath(ModDirectoryName).ToSmbFile();
            if(!fileHandle.Exists)
                return new Mods.FileCluster[0];

            return fileHandle
                .Items
                .Where(item => item.IsDirectory || item.Extension.ToLower() == ".zip")
                .Select
                (item => Mods.FileCluster.Create(item.FullName, AllPaths, ModConfiguration, Parent))
                .Where(item => item != null)
                .ToArray();
        }

        IDictionary<string, bool> GetModConfiguration()
        {
            var fileHandle = FilesPath(ModDirectoryName)
                .PathCombine(ModConfigurationFileName).ToSmbFile();

            if(!fileHandle.Exists)
                return new Dictionary<string, bool>();

            var text = fileHandle.String;
            var result = text.FromJson<ModListJSon>();
            var modConfigurationCells = result.Cells;
            return modConfigurationCells.ToDictionary(item => item.Name, item => item.IsEnabled);
        }

        public IEnumerable<Mods.FileCluster> ModFiles => ModFilesCache.Value;
        public IEnumerable<Saves.FileCluster> SaveFiles => SaveFilesCache.Value;
        IDictionary<string, bool> ModConfiguration => ModConfigurationCache.Value;

        public IEnumerable<ModConflict> SaveFileConflicts 
            => SaveFiles
            .SelectMany(save => save.Conflicts);

        public void InitializeFrom(UserConfiguration source)
            =>
            source.FilesPath(PlayerDataFileName).ToSmbFile().CopyTo(FilesPath(PlayerDataFileName));

        protected override string GetNodeDump() => Path.ToSmbFile().Name;

    }
}