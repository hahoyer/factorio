using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using log4net;

namespace ManageModsAndSavefiles
{
    public sealed class UserConfiguration : DumpableObject
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        const string SaveDirectoryName = "saves";
        internal const string ModDirectoryName = "mods";
        const string PlayerDataFileName = "player-data.json";
        const string ReadDataTag = "read-data";
        const string ModConfigurationFileName = "mod-list.json";

        internal static string[] Paths
            => Extension
                .SystemWriteDataDir
                .FileHandle()
                .RecursiveItems()
                .Where(IsRelevantPathCandidate)
                .Select(item => item.FullName)
                .ToArray();

        static bool IsRelevantPathCandidate(File item)
            =>
            item.IsDirectory
            && IsExistent(item, PlayerDataFileName, false)
            && IsExistent(item, SaveDirectoryName, true)
            && IsExistent(item, ModDirectoryName, true);

        static bool IsExistent(File item, string fileName, bool isDictionary)
        {
            var fileHandle = item.FullName.PathCombine(fileName).FileHandle();
            return fileHandle.Exists && fileHandle.IsDirectory == isDictionary;
        }

        internal static UserConfiguration Create(string item, string[] allPaths)
            => new UserConfiguration(item, allPaths);

        readonly string Path;
        readonly string[] AllPaths;
        readonly ValueCache<SaveFile[]> SaveFilesCache;
        readonly ValueCache<ModFile[]> ModFilesCache;
        readonly ValueCache<IDictionary<string,bool>> ModConfigurationCache;

        UserConfiguration(string path, string[] allPaths)
        {
            Path = path;
            AllPaths = allPaths;
            ModConfigurationCache = new ValueCache<IDictionary<string, bool>>(GetModConfiguration);
            ModFilesCache = new ValueCache<ModFile[]>(GetModFiles);
            SaveFilesCache = new ValueCache<SaveFile[]>(GetSaveFiles);
        }

        string FilesPath(string item) => Path.PathCombine(item);

        SaveFile[] GetSaveFiles()
        {
            var fileHandle = FilesPath(SaveDirectoryName).FileHandle();
            if(!fileHandle.Exists)
                return new SaveFile[0];

            return fileHandle
                .Items
                .Where(item => !item.IsDirectory && item.Extension.ToLower() == ".zip")
                .Select(item => new SaveFile(item.FullName))
                .ToArray();
        }

        ModFile[] GetModFiles()
        {
            var fileHandle = FilesPath(ModDirectoryName).FileHandle();
            if(!fileHandle.Exists)
                return new ModFile[0];

            return fileHandle
                .Items
                .Where(item => item.IsDirectory || item.Extension.ToLower() == ".zip")
                .Select(item => ModFile.Create(item.FullName, AllPaths, ModConfiguration))
                .ToArray();
        }

        IDictionary<string, bool> GetModConfiguration()
        {
            var fileHandle = FilesPath(ModDirectoryName)
                .PathCombine(ModConfigurationFileName)
                .FileHandle();

            if(!fileHandle.Exists)
                return new Dictionary<string, bool>();

            var text = fileHandle.String;
            var result = text.FromJson<ModConfiguration>();
            var modConfigurationCells = result.Cells;
            return modConfigurationCells.ToDictionary(item=>item.Name, item=>item.IsEnabled);
        }

        internal IEnumerable<ModFile> ModFiles => ModFilesCache.Value;
        IEnumerable<SaveFile> SaveFiles => SaveFilesCache.Value;
        IDictionary<string, bool> ModConfiguration => ModConfigurationCache.Value;

        public void InitializeFrom(UserConfiguration source)
        {
            Log.Debug("InitializeFrom");
            source.FilesPath(PlayerDataFileName).FileHandle().CopyTo(FilesPath(PlayerDataFileName));
        }

        internal interface INameProvider
        {
            string Name { get; }
        }

        protected override string GetNodeDump() => Path.FileHandle().Name;
    }
}