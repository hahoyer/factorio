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
       internal const string SaveDirectoryName = "saves";
        internal const string ModDirectoryName = "mods";
        internal const string LogfileName= "factorio-current.log";
        internal const string PreviousLogfileName = "factorio-previous.log";
        const string PlayerDataFileName = "player-data.json";
        const string ReadDataTag = "read-data";
        const string ModConfigurationFileName = "mod-list.json";

        internal static SmbFile[] Paths(SmbFile root)
            => root
                .RecursiveItems()
                .Where(IsRelevantPathCandidate)
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
            (SmbFile item, SmbFile[] allPaths, MmasfContext parent)
            => new UserConfiguration(item, allPaths, parent);

        public readonly SmbFile Path;
        readonly SmbFile[] AllPaths;
        readonly ValueCache<Saves.FileCluster[]> SaveFilesCache;
        readonly ValueCache<Mods.FileCluster[]> ModFilesCache;
        readonly ValueCache<IDictionary<string, bool>> ModConfigurationCache;
        readonly MmasfContext Parent;
        readonly LogfileWatcher LogfileWatcher;

        public string Name => Path.Name;
        public bool IsRoot => Path.FullName == Parent.DataConfiguration.RootUserConfigurationPath;
        public bool IsCurrent 
            => Path.FullName == Parent.DataConfiguration.CurrentUserConfigurationPath.FullName;

        UserConfiguration(SmbFile path, SmbFile[] allPaths, MmasfContext parent)
        {
            Path = path;
            AllPaths = allPaths;
            Parent = parent;
            ModConfigurationCache = new ValueCache<IDictionary<string, bool>>(GetModConfiguration);
            ModFilesCache = new ValueCache<Mods.FileCluster[]>(GetModFiles);
            SaveFilesCache = new ValueCache<Saves.FileCluster[]>(GetSaveFiles);
            return;
            LogfileWatcher = new LogfileWatcher(Path);
        }

        SmbFile FilesPath(string item) => Path.PathCombine(item);

        Saves.FileCluster[] GetSaveFiles()
        {
            var fileHandle = FilesPath(SaveDirectoryName);
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
            var fileHandle = FilesPath(ModDirectoryName);
            if(!fileHandle.Exists)
                return new Mods.FileCluster[0];

            return fileHandle
                .Items
                .Where(item => item.IsDirectory || item.Extension.ToLower() == ".zip")
                .Select
                (item => Mods.FileCluster.Create(item, AllPaths, ModConfiguration, Parent))
                .Where(item => item != null)
                .ToArray();
        }

        IDictionary<string, bool> GetModConfiguration()
        {
            var fileHandle = FilesPath(ModDirectoryName)
                .PathCombine(ModConfigurationFileName);

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
                source.FilesPath(PlayerDataFileName)
                    .CopyTo(FilesPath(PlayerDataFileName).FullName);

        protected override string GetNodeDump() => Path.Name;
    }
}