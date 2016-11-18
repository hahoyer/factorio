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
        internal readonly bool IsEnabled;

        ModFile(string path, int configIndex, bool isEnabled)
        {
            File = path.FileHandle();
            ModName = GetModNameFromFileName();
            Version = GetVersionFromFile();
            ConfigIndex = configIndex;
            IsEnabled = isEnabled;
        }

        string UserConfiguration.INameProvider.Name => ModName;

        public static ModFile Create(string path, IEnumerable<string> paths, bool isEnabled)
        {
            var dictionary = path
                .FileHandle().DirectoryName
                .FileHandle().DirectoryName;

            var index = paths
                .OrderByDescending(item => item.Split('\\').Length)
                .ThenBy(item => item)
                .IndexWhere(dictionary.StartsWith)
                .AssertValue();

            return new ModFile(path, index, isEnabled);
        }

        string GetVersionFromFileName()
        {
            var nameParts = File.Name.Split('_');
            if(nameParts.Length == 1)
                return "<unknown>";

            return File.Name.Substring(nameParts[0].Length + 1);
        }

        string GetVersionFromFile()
        {
            var text =
                File.IsDirectory ? GetInfoJSonFromDirectory() : GetInfoJSonFromZipFile();
            if(text == null)
                return "<unknown>";

            var info = text.FromJson<ModInfo>();
            return info.Version;
        }

        string GetInfoJSonFromZipFile()
        {
            var headerDir = File.Name.Substring(0, File.Name.Length - 4);
            return File
                .FullName
                .ZipFileHandle()
                .GetItem(headerDir + "/" + FileNameInfoJson)
                .String;
        }

        string GetInfoJSonFromDirectory()
            => File.FullName.PathCombine(FileNameInfoJson)
                .FileHandle()
                .String;

        string GetModNameFromFileName() => File.Name.Split('_')[0];

        public override string ToString()
            => ConfigIndex + ":" +
            ModName + " " +
            Version;
    }
}