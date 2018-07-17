using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                : this(Map[path[0]], path.Skip(1).Stringify("\\")) {}

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
                    var key = path.Take(path.Length - 1).Stringify("\\");
                    var value = path.Last();

                    using(var item = Root.OpenSubKey(key))
                        return item != null && item.GetValueNames().Any(name => name == value);
                }
            }

            public T GetValue<T>()
            {
                var path = Key.Split('\\');
                var key = path.Take(path.Length - 1).Stringify("\\");
                var value = path.Last();

                using(var item = Root.OpenSubKey(key))
                    return (T) item?.GetValue(value);
            }
        }

        internal const string SystemReadDataPlaceholder = "__PATH__system-read-data__";

        const string SystemWriteDataPlaceholder = "__PATH__system-write-data__";

        internal static readonly SmbFile SystemWriteDataDir
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine("Factorio")
                .ToSmbFile();

        internal static IEnumerable<SmbFile> FindFilesThatEndsWith
            (this SmbFile root, string target)
            => new[] {root}.FindFilesThatEndsWith(target);

        internal static IEnumerable<SmbFile> FindFilesThatEndsWith
            (this IEnumerable<SmbFile> root, string target)
            => root.SelectMany(f => f.RecursiveItems())
                .Where(item => item.FullName.EndsWith(target, true, null));

        static FileIniDataParser CreateFileIniDataParser(string commentString)
        {
            var result = new FileIniDataParser();
            result.Parser.Configuration.CommentString = commentString;
            result.Parser.Configuration.AssigmentSpacer = "";
            return result;
        }

        internal static IniData FromIni
            (this SmbFile name, string commentString)
            => CreateFileIniDataParser(commentString).ReadFile(name.FullName);

        internal static void SaveTo(this IniData data, SmbFile name, string commentString)
            => CreateFileIniDataParser(commentString).WriteFile(name.FullName, data);

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
            name.Replace(SystemWriteDataDir.FullName, SystemWriteDataPlaceholder)
                .Replace("\\", "/");

        internal static SmbFile PathFromFactorioStyle(this string name) =>
            name.Replace(SystemWriteDataPlaceholder, SystemWriteDataDir.FullName)
                .Replace("/", "\\")
                .ToSmbFile();

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

        public static string Left(this string target, int targetLength, string pad = " ")
        {
            for(var delta = targetLength - target.Length; delta > 0; delta -= pad.Length)
                target += pad;
            return target.Substring(0, targetLength);
        }

        public static string Right(this string target, int targetLength, string pad = " ")
        {
            var delta = targetLength - target.Length;
            for(; delta > 0; delta -= pad.Length)
                target = pad + target;
            return target.Substring(-delta);
        }
    }
}