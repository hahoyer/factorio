﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using hw.DebugFormatter;
using hw.Helper;
using Newtonsoft.Json;

namespace Common
{
    public static class Extension
    {
        public static byte[] AsciiToByteArray(this string value) => Encoding.ASCII.GetBytes(value);

        public static void Sleep(this TimeSpan value) => Thread.Sleep(value);

        public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);
        public static void WriteLine(this string value) => Tracer.Line(value);

        public static T FromJson<T>(this string jsonText)
            => JsonConvert.DeserializeObject<T>(jsonText);

        public static object FromJson(this string jsonText, Type resultType)
            => JsonConvert.DeserializeObject(jsonText, resultType);

        public static string ToJson<T>(this T o)
            => JsonConvert.SerializeObject(o, Formatting.Indented);

        public static T FromJsonFile<T>(this string jsonFileName)
            where T : class
            => jsonFileName.FileHandle().String?.FromJson<T>();

        public static void ToJsonFile<T>(this string jsonFileName, T o)
            where T : class
            => jsonFileName.FileHandle().String = o.ToJson();

        public static string UnescapeComma(this string value)
            => value
                .Replace("&comma;", ",")
                .Replace("&ampersant;", "&");

        public static string EscapeComma(this string value)
            => value
                .Replace("&", "&ampersant;")
                .Replace(",", "&comma;");

        public static IEnumerable<T> EnsureAny<T>(this IEnumerable<T> value, Action onError)
        {
            var isAny = false;
            foreach(var item in value)
            {
                yield return item;

                isAny = true;
            }

            if(isAny)
                yield break;

            onError();
        }

        public static IEnumerable<T> EnsureNoDuplicate<T>(this IEnumerable<T> value, Action onError)
        {
            var isAny = false;
            foreach(var item in value)
            {
                if(isAny)
                {
                    onError();
                    yield break;
                }

                yield return item;

                isAny = true;
            }
        }
    }
}