using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json;

namespace ManageModsAndSavefiles
{
    public static class Extension
    {
        const string SystemWriteDataPlaceholder = "__PATH__system-write-data__";
        internal const string SystemReadDataPlaceholder = "__PATH__system-read-data__";

        internal static readonly string SystemWriteDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine("Factorio");

        internal static IEnumerable<File> FindFilesThatEndsWith(this File root, string target)
            => root.RecursiveItems()
                .Where(item => item.FullName.EndsWith(target));

        static readonly FileIniDataParser IniParserInstance = CreateFileIniDataParser();

        static FileIniDataParser CreateFileIniDataParser()
        {
            var result = new FileIniDataParser();
            result.Parser.Configuration.CommentString = "#";
            return result;
        }

        internal static IniData FromIni(this string name) => IniParserInstance.ReadFile(name);

        internal static void SaveTo(this IniData data, string name)
            => IniParserInstance.WriteFile(name, data);

        internal static T FromJson<T>(this string jsonText)
            => JsonConvert.DeserializeObject<T>(jsonText);

        internal static string ToJson<T>(this T o)
            => JsonConvert.SerializeObject(o, Formatting.Indented);

        internal static T FromJsonFile<T>(this string jsonFileName)
            where T : class
        => jsonFileName.FileHandle().String?.FromJson<T>();

        internal static void ToJsonFile<T>(this string jsonFileName, T o)
            where T : class
        => jsonFileName.FileHandle().String = o.ToJson();

        internal static string PathToFactorioStyle(this string name) =>
            name.Replace(SystemWriteDataDir, SystemWriteDataPlaceholder)
                .Replace("\\", "/");

        internal static string PathFromFactorioStyle(this string name) =>
            name.Replace(SystemWriteDataPlaceholder, SystemWriteDataDir)
                .Replace("/", "\\");

        public static ZipFileHandle ZipFileHandle(this string name) => new ZipFileHandle(name, null);


        public static TValue? GetValueOrNull<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary, TKey target)
            where TValue: struct
        {
            TValue result;
            if(dictionary.TryGetValue(target, out result))
                return result;

            return null;
        }

        public static TValue GetValueOrDefault<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary, TKey target)
        {
            TValue result;
            if (dictionary.TryGetValue(target, out result))
                return result;

            return default(TValue);
        }

    }
}