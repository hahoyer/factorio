using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using hw.DebugFormatter;
using hw.Helper;
using IniParser;
using IniParser.Model;
using ManageModsAndSavefiles.Compression;
using ManageModsAndSavefiles.Compression.Microsoft;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ManageModsAndSavefiles
{
    public static class Extension
    {
        internal sealed class RegistryItem
        {
            static readonly IDictionary<string, RegistryKey> Map
                = new Dictionary<string, RegistryKey>
                {
                    {"HKEY_CLASSES_ROOT", Microsoft.Win32.Registry.ClassesRoot},
                    {"HKEY_CURRENT_USER", Microsoft.Win32.Registry.CurrentUser},
                    {"HKEY_LOCAL_MACHINE", Microsoft.Win32.Registry.LocalMachine},
                    {"HKEY_CURRENT_CONFIG", Microsoft.Win32.Registry.CurrentConfig}
                };

            readonly string Key;

            readonly RegistryKey Root;

            public RegistryItem(RegistryKey root, string key)
            {
                Root = root;
                Key = key;
            }

            public RegistryItem(string[] path)
                : this(Map[path[0]], path.Skip(count: 1).Stringify(separator: "\\"))
            {
            }

            public bool IsValidSubKey
            {
                get
                {
                    using(var item = Root.OpenSubKey(Key))
                        return item != null;
                }
            }

            public bool IsValidValue
            {
                get
                {
                    var path = Key.Split('\\');
                    var key = path.Take(path.Length - 1).Stringify(separator: "\\");
                    var value = path.Last();

                    using(var item = Root.OpenSubKey(key))
                        return item != null && item.GetValueNames().Any(name => name == value);
                }
            }

            public T GetValue<T>()
            {
                var path = Key.Split('\\');
                var key = path.Take(path.Length - 1).Stringify(separator: "\\");
                var value = path.Last();

                using(var item = Root.OpenSubKey(key))
                    return (T) item?.GetValue(value);
            }
        }

        const string SystemWriteDataPlaceholder = "__PATH__system-write-data__";
        internal const string SystemReadDataPlaceholder = "__PATH__system-read-data__";

        internal static readonly string SystemWriteDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine("Factorio");

        internal static IEnumerable<SmbFile> FindFilesThatEndsWith
            (this SmbFile root, string target)
            => new[] {root}.FindFilesThatEndsWith(target);

        internal static IEnumerable<SmbFile> FindFilesThatEndsWith
            (this IEnumerable<SmbFile> root, string target)
            => root.SelectMany(f => f.RecursiveItems())
                .Where(item => item.FullName.EndsWith(target));

        static FileIniDataParser CreateFileIniDataParser(string commentString)
        {
            var result = new FileIniDataParser();
            result.Parser.Configuration.CommentString = commentString;
            result.Parser.Configuration.AssigmentSpacer = "";
            return result;
        }

        internal static IniData FromIni
            (this string name, string commentString) 
            => CreateFileIniDataParser(commentString).ReadFile(name);

        internal static void SaveTo(this IniData data, string name, string commentString)
            => CreateFileIniDataParser(commentString).WriteFile(name, data);

        internal static T FromJson<T>(this string jsonText)
            => JsonConvert.DeserializeObject<T>(jsonText);

        internal static string ToJson<T>(this T o)
            => JsonConvert.SerializeObject
            (
                o,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                });

        internal static T FromJsonFile<T>(this string jsonFileName)
            where T : class
            => jsonFileName.ToSmbFile().String?.FromJson<T>();

        internal static void ToJsonFile<T>(this string jsonFileName, T o)
            where T : class
            => jsonFileName.ToSmbFile().String = o.ToJson();

        internal static string PathToFactorioStyle(this string name) =>
            name.Replace(SystemWriteDataDir, SystemWriteDataPlaceholder)
                .Replace(oldValue: "\\", newValue: "/");

        internal static string PathFromFactorioStyle(this string name) =>
            name.Replace(SystemWriteDataPlaceholder, SystemWriteDataDir)
                .Replace(oldValue: "/", newValue: "\\");

        public static IZipArchiveHandle ZipHandle(this string name, bool quirks = false)
            =>
                quirks
                    ? (IZipArchiveHandle) new ZipArchiveHandle(name)
                    : new Compression.Nuget.ZipArchiveHandle(name);

        public static TValue? GetValueOrNull<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary, TKey target)
            where TValue : struct
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
            if(dictionary.TryGetValue(target, out result))
                return result;

            return default(TValue);
        }

        internal static RegistryItem RegistryCurrentUser(this string fullKey)
            => new RegistryItem(Microsoft.Win32.Registry.CurrentUser, fullKey);

        internal static RegistryItem Registry(this string fullKey)
            => new RegistryItem(fullKey.Split('\\'));

        public static void Sleep(this TimeSpan value) => Thread.Sleep(value);

        public static TimeSpan MilliSeconds(this int value) => TimeSpan.FromMilliseconds(value);
        public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);
        public static void WriteLine(this string value) => Tracer.Line(value);
        public static void WriteFlaggedLine(this string value) => Tracer.FlaggedLine(value, stackFrameDepth: 1);

    }
}