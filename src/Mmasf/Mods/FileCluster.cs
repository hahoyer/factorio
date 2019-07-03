using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace ManageModsAndSaveFiles.Mods
{
    public sealed class FileCluster : DumpableObject
    {
        const string FileNameInfoJson = "info.json";

        public static FileCluster Create
        (
            SmbFile path,
            SmbFile[] paths,
            IDictionary<string, bool> knownMods,
            MmasfContext parent)
        {
            var infoJSon = GetInfoJSon(path);
            if(infoJSon == null)
                return null;

            var dictionary = path.Directory.Directory;

            var index = paths
                .OrderByDescending(item => item.FullName.Split('\\').Length)
                .ThenBy(item => item.FullName)
                .IndexWhere(file => file.Contains(dictionary))
                .AssertValue();

            var modName = infoJSon.Name;
            var isEnabled = knownMods.GetValueOrNull(modName);
            var version = new Version(infoJSon.Version);
            var description = parent.ModDictionary[modName][version];

            description.InfoJSon = infoJSon;
            return new FileCluster(path, isEnabled, index, description, infoJSon);
        }

        static Version GetVersionFromFile(SmbFile file)
        {
            var text =
                file.IsDirectory ? GetInfoJSonFromDirectory(file) : GetInfoJSonFromZipFile(file);
            if(text == null)
                return new Version();

            var info = text.FromJson<InfoJSon>();
            return new Version(info.Version);
        }

        static string GetModNameFromFile(SmbFile file)
        {
            var text =
                file.IsDirectory ? GetInfoJSonFromDirectory(file) : GetInfoJSonFromZipFile(file);
            if(text == null)
                return "<unknown>";

            var info = text.FromJson<InfoJSon>();
            return info.Name;
        }

        static InfoJSon GetInfoJSon(SmbFile file)
        {
            var text =
                file.IsDirectory
                    ? GetInfoJSonFromDirectory(file)
                    : GetInfoJSonFromZipFile(file);

            return text?.FromJson<InfoJSon>();
        }

        static string GetInfoJSonFromZipFile(SmbFile modFileFile)
        {
            try
            {
                return GetInfoJSonFromZipFile(modFileFile, false);
            }
            catch
            {
                return GetInfoJSonFromZipFile(modFileFile, true);
            }
        }

        static string GetInfoJSonFromZipFile(SmbFile modFileFile, bool quirks)
        {
            var headerDir = modFileFile.Name.Substring(0, modFileFile.Name.Length - 4);
            return modFileFile
                .FullName
                .ZipHandle(quirks)
                .Items
                .Single(item => item.ItemName == FileNameInfoJson && item.Depth == 2)
                .String;
        }

        static string GetInfoJSonFromDirectory(SmbFile file)
            => file.FullName.PathCombine(FileNameInfoJson).ToSmbFile()
                .String;

        public readonly int ConfigIndex;
        public readonly SmbFile File;
        public readonly ModDescription Description;
        public readonly InfoJSon InfoJSon;
        public readonly bool? IsEnabled;

        public string Name =>InfoJSon.Name;
        public string Title=> InfoJSon.Title;
        public Version Version => new Version(InfoJSon.Version);

        FileCluster(SmbFile fileHandle, bool? isEnabled, int configIndex, ModDescription description, InfoJSon infoJSon)
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