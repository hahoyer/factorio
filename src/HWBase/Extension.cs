using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HWBase;

public static class Extension
{
    public sealed class RegistryItem
    {
        static readonly IDictionary<string, RegistryKey> Map
            = new Dictionary<string, RegistryKey>
            {
                { "HKEY_CLASSES_ROOT", Microsoft.Win32.Registry.ClassesRoot }
                , { "HKEY_CURRENT_USER", Microsoft.Win32.Registry.CurrentUser }
                , { "HKEY_LOCAL_MACHINE", Microsoft.Win32.Registry.LocalMachine }
                , { "HKEY_CURRENT_CONFIG", Microsoft.Win32.Registry.CurrentConfig }
            };

        readonly string Key;

        readonly RegistryKey Root;

        public RegistryItem(RegistryKey root, string key)
        {
            Root = root;
            Key = key;
        }

        public RegistryItem(string[] path)
            : this(Map[path[0]], path.Skip(1).Stringify("\\")) { }

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
                return (T)item?.GetValue(value);
        }
    }

    internal static RegistryItem RegistryCurrentUser(this string fullKey)
        => new(Microsoft.Win32.Registry.CurrentUser, fullKey);

    public static RegistryItem Registry(this string fullKey) => new(fullKey.Split('\\'));

    public static TimeSpan MilliSeconds(this int value) => TimeSpan.FromMilliseconds(value);
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

    public static string ToJSon<T>(this T o)
        => JsonConvert.SerializeObject
        (
            o,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });

    public static T FromJsonFile<T>(this string jsonFileName)
        where T : class
        => jsonFileName.ToSmbFile().String?.FromJson<T>();

    public static JObject FromJsonFile(this string jsonFileName)
        => jsonFileName.ToSmbFile().String?.FromJson();

    public static JObject FromJson(this string jsonText)
        => JObject.Parse(jsonText);

    public static void ToJsonFile<T>(this string jsonFileName, T o)
        where T : class
        => jsonFileName.ToSmbFile().String = o.ToJSon();

    public static T FromJson<T>(this string jsonText, params JsonConverter[] converters)
        => JsonConvert.DeserializeObject<T>(jsonText, converters);

    public static object FromJson(this string jsonText, Type resultType)
        => JsonConvert.DeserializeObject(jsonText, resultType);
}