using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class ModFile : DumpableObject, UserConfiguration.INameProvider
    {
        internal readonly string ModName;
        internal readonly string Version;
        internal readonly int ConfigIndex;
        readonly File File;

        public static ModFile Create(string path, string[] paths)
        {
            var dictionary = path
                .FileHandle().DirectoryName
                .FileHandle().DirectoryName;

            var index = paths
                .OrderByDescending(item => item.Split('\\').Length)
                .ThenBy(item => item)
                .IndexWhere(dictionary.StartsWith)
                .AssertValue();

            return new ModFile(path, index);
        }

        ModFile(string path, int configIndex)
        {
            File = path.FileHandle();
            ModName = GetModNameFromFileName();
            Version = GetVersionFromFile();
            ConfigIndex = configIndex;
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
            var info = text.FromJson<ModInfo>();
            return null;
        }

        string GetInfoJSonFromZipFile()
        {
            var headerDir = File.Name.Substring(0, File.Name.Length - 4);
            return File
                .FullName
                .ZipFileHandle()
                .GetItem(headerDir + "/info.json")
                .String;
        }

        string GetInfoJSonFromDirectory()
            => File.FullName.PathCombine("info.json")
                .FileHandle()
                .String;

        string GetModNameFromFileName() => File.Name.Split('_')[0];

        string UserConfiguration.INameProvider.Name => ModName;

        public override string ToString()
            => ConfigIndex + ":" +
            ModName + " " +
            Version;
    }

    class ModInfo
    {
        public string name;
        public string version;
        public string factorio_version;
        public string title;
        public string author;
        public string contact;
        public string homepage;
        public string description;
    }
}