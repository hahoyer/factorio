using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles.Mods
{
    public sealed class FileCluster : DumpableObject
    {
        const string FileNameInfoJson = "info.json";

        public static FileCluster Create
        (
            string path,
            IEnumerable<string> paths,
            IDictionary<string, bool> knownMods,
            MmasfContext parent)
        {
            var fileHandle = path.FileHandle();
            var infoJSon = GetInfoJSon(fileHandle);
            if(infoJSon == null)
                return null;

            var dictionary = path
                .FileHandle().DirectoryName
                .FileHandle().DirectoryName;

            var index = paths
                .OrderByDescending(item => item.Split('\\').Length)
                .ThenBy(item => item)
                .IndexWhere(dictionary.StartsWith)
                .AssertValue();

            var modName = infoJSon.Name;
            var isEnabled = knownMods.GetValueOrNull(modName);
            var version = new Version(infoJSon.Version);
            var description = parent.ModDictionary[modName][version];

            description.InfoJSon = infoJSon;
            return new FileCluster(fileHandle, isEnabled, index, description, infoJSon);
        }

        static Version GetVersionFromFile(File file)
        {
            var text =
                file.IsDirectory ? GetInfoJSonFromDirectory(file) : GetInfoJSonFromZipFile(file);
            if(text == null)
                return new Version();

            var info = text.FromJson<InfoJSon>();
            return new Version(info.Version);
        }

        static string GetModNameFromFile(File file)
        {
            var text =
                file.IsDirectory ? GetInfoJSonFromDirectory(file) : GetInfoJSonFromZipFile(file);
            if(text == null)
                return "<unknown>";

            var info = text.FromJson<InfoJSon>();
            return info.Name;
        }

        static InfoJSon GetInfoJSon(File file)
        {
            var text =
                file.IsDirectory
                    ? GetInfoJSonFromDirectory(file)
                    : GetInfoJSonFromZipFile(file);

            return text?.FromJson<InfoJSon>();
        }

        static string GetInfoJSonFromZipFile(File modFileFile)
        {
            var headerDir = modFileFile.Name.Substring(0, modFileFile.Name.Length - 4);
            return modFileFile
                .FullName
                .ZipHandle()
                .Items
                .Single(item=>item.ItemName == FileNameInfoJson && item.Depth == 2)
                .String;
        }

        static string GetInfoJSonFromDirectory(File file)
            => file.FullName.PathCombine(FileNameInfoJson)
                .FileHandle()
                .String;

        public readonly int ConfigIndex;
        public readonly File File;
        public readonly ModDescription Description;
        public readonly InfoJSon InfoJSon;
        public readonly bool? IsEnabled;

        FileCluster(File fileHandle, bool? isEnabled, int configIndex, ModDescription description, InfoJSon infoJSon)
        {
            File = fileHandle;
            ConfigIndex = configIndex;
            Description = description;
            InfoJSon = infoJSon;
            IsEnabled = isEnabled;
        }

        public override string ToString() => ConfigIndex + ":" + Description;
    }
}