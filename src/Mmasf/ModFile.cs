using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class ModFile : DumpableObject, UserConfiguration.INameProvider
    {
        const string FileNameInfoJson = "info.json";
        internal readonly int ConfigIndex;
        readonly File File;
        internal readonly string ModName;
        internal readonly string Version;
        internal readonly bool? IsEnabled;

        ModFile(File fileHandle, string modName, string version, bool? isEnabled, int configIndex)
        {
            File = fileHandle;
            ModName = modName;
            Version = version;
            ConfigIndex = configIndex;
            IsEnabled = isEnabled;
        }

        string UserConfiguration.INameProvider.Name => ModName;

        public static ModFile Create
        (
            string path,
            IEnumerable<string> paths,
            IDictionary<string, bool> knownMods
        )
        {
            var dictionary = path
                .FileHandle().DirectoryName
                .FileHandle().DirectoryName;

            var index = paths
                .OrderByDescending(item => item.Split('\\').Length)
                .ThenBy(item => item)
                .IndexWhere(dictionary.StartsWith)
                .AssertValue();

            var fileHandle = path.FileHandle();
            var modName = GetModNameFromFile(fileHandle);
            var isEnabled = knownMods.GetValueOrNull(modName);

            return new ModFile
            (
                fileHandle,
                modName,
                GetVersionFromFile(fileHandle),
                isEnabled,
                index
            );
        }

        static string GetVersionFromFile(File file)
        {
            var text =
                file.IsDirectory ? GetInfoJSonFromDirectory(file) : GetInfoJSonFromZipFile(file);
            if(text == null)
                return "<unknown>";

            var info = text.FromJson<ModInfo>();
            return info.Version;
        }

        static string GetModNameFromFile(File file)
        {
            var text =
                file.IsDirectory ? GetInfoJSonFromDirectory(file) : GetInfoJSonFromZipFile(file);
            if (text == null)
                return "<unknown>";

            var info = text.FromJson<ModInfo>();
            return info.Name;
        }

        static string GetInfoJSonFromZipFile(File modFileFile)
        {
            var headerDir = modFileFile.Name.Substring(0, modFileFile.Name.Length - 4);
            return modFileFile
                .FullName
                .ZipFileHandle()
                .GetItem(headerDir + "/" + FileNameInfoJson)
                .String;
        }

        static string GetInfoJSonFromDirectory(File file)
            => file.FullName.PathCombine(FileNameInfoJson)
                .FileHandle()
                .String;

        public override string ToString()
            => ConfigIndex + ":" +
            ModName + " " +
            Version;
    }
}