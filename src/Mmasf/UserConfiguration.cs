using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class UserConfiguration : DumpableObject
    {
        const string ConfigurationIniFileName = "config.ini";
        const string ConfigurationDirectoryName = "config";
        const string SaveDirectoryName = "saves";
        const string ModDirectoryName = "mods";
        const string PathSectionName = "path";
        const string ReadDataTag = "read-data";
        const string WriteDataTag = "write-data";

        internal static readonly string OriginalUserPath
            = Extension.SystemWriteDataDir.PathCombine(ConfigurationDirectoryName)
                .FileHandle()
                .FullName;

        public static readonly UserConfiguration Original =
            Create(Configuration.Instance.OriginalUserPath);

        public static readonly UserConfiguration Current =
            Create(SystemConfiguration.Instance.CurrentConfigurationPath);

        static UserConfiguration Create(string path)
        {
            var iniFile = new IniFile(path.PathCombine(ConfigurationIniFileName));
            return new UserConfiguration(iniFile);
        }

        readonly IniFile IniFile;
        readonly ValueCache<SaveFile[]> SaveFilesCache;
        readonly ValueCache<ModFile[]> ModFilesCache;

        UserConfiguration(IniFile iniFile)
        {
            IniFile = iniFile;
            ModFilesCache = new ValueCache<ModFile[]>(GetModFiles);
            SaveFilesCache = new ValueCache<SaveFile[]>(GetSaveFiles);
        }

        string FilesPath(string item)
            => IniFile[PathSectionName][WriteDataTag]
                .PathFromFactorioStyle()
                .PathCombine(item);

        SaveFile[] GetSaveFiles()
            => FilesPath(SaveDirectoryName)
                .FileHandle()
                .Items
                .Where(item => !item.IsDirectory && item.Extension.ToLower() == ".zip")
                .Select(item => new SaveFile(item.FullName))
                .ToArray();

        ModFile[] GetModFiles()
            => FilesPath(ModDirectoryName)
                .FileHandle()
                .Items
                .Where(item => item.IsDirectory || item.Extension.ToLower() == ".zip")
                .Select(item => new ModFile(item.FullName))
                .ToArray();

        IEnumerable<ModFile> ModFiles => ModFilesCache.Value;
        IEnumerable<SaveFile> SaveFiles => SaveFilesCache.Value;

        public void InitializeFrom(UserConfiguration source)
        {
            IniFile.UpdateFrom(source.IniFile);
            CorrectPaths();
            Synchronize(SaveFiles, source.SaveFiles, SaveDirectoryName, source);
            Synchronize(ModFiles, source.ModFiles, ModDirectoryName, source);
        }

        void Synchronize<T>
        (
            IEnumerable<T> currentFiles,
            IEnumerable<T> masterFiles,
            string itemName,
            UserConfiguration master
        )
            where T : class, INameProvider
        {
            var pathOfCurrent = FilesPath(itemName);
            var pathOfMaster = master.FilesPath(itemName);
            var merge = currentFiles
                .Merge(masterFiles, item => item.Name)
                .ToArray();

            var fileNamesToGet = merge
                .Where(item => item.Item2 == null && item.Item3 != null)
                .Select(item => item.Item1)
                .ToArray();

            foreach(var fileName in fileNamesToGet)
            {
                var sourceFileName = pathOfMaster.PathCombine(fileName);
                var destFileName = pathOfCurrent.PathCombine(fileName);

                Tracer.LinePart("Copying " + sourceFileName + " to " + destFileName + " ... ");
                sourceFileName.FileHandle().CopyTo(destFileName);
                Tracer.Line("complete");
            }

            var fileNamesToPut = merge
                .Where(item => item.Item2 != null && item.Item3 == null)
                .Select(item => item.Item1)
                .ToArray();

            foreach(var fileName in fileNamesToPut)
            {
                var sourceFileName = pathOfCurrent.PathCombine(fileName);
                var destFileName = pathOfMaster.PathCombine(fileName);

                Tracer.LinePart("Copying " + sourceFileName + " to " + destFileName + " ... ");
                sourceFileName.FileHandle().CopyTo(destFileName);
                Tracer.Line("complete");
            }
        }

        internal interface INameProvider
        {
            string Name { get; }
        }

        void CorrectPaths()
        {
            var pathSection = IniFile[PathSectionName];
            pathSection[ReadDataTag] = Extension.SystemReadDataPlaceholder;
            pathSection[WriteDataTag] = IniFile
                .Path
                .FileHandle()
                .DirectoryName
                .FileHandle()
                .DirectoryName
                .PathToFactorioStyle();
            IniFile.Persist();
        }
    }
}