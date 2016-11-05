using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using IniParser;
using IniParser.Model;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public static class Extension
    {
        public static IEnumerable<File> RecursiveItems(this File root)
        {
            if(!root.IsDirectory)
            {
                yield return root;

                yield break;
            }

            IEnumerable<File> files = new[] {root};
            while(true)
            {
                var newList = new List<File>();
                foreach(var item in files.SelectMany(GuardedItems))
                {
                    yield return item;

                    if(item.IsDirectory)
                        newList.Add(item);
                }

                if(!newList.Any())
                    yield break;

                files = newList;
            }
        }

        static File[] GuardedItems(this File item)
        {
            try
            {
                if(item.IsDirectory)
                    return item.Items;
            }
            catch {}

            return new File[0];
        }

        internal static IEnumerable<File> Find(this File p, string target)
            => p
                .RecursiveItems()
                .Where(item => item.FullName.EndsWith(target));

        static readonly FileIniDataParser IniParserInstance = CreateFileIniDataParser();
        static readonly JavaScriptSerializer JavaScriptSerializer = new JavaScriptSerializer();

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
            => JavaScriptSerializer.Deserialize<T>(jsonText);

        internal static string ToJson<T>(this T o)
            => JavaScriptSerializer.Serialize(o);

        internal static T FromJsonFile<T>(this string jsonFileName)
            where T : class
        => jsonFileName.FileHandle().String?.FromJson<T>();

        internal static void ToJsonFile<T>(this string jsonFileName, T o)
            where T : class
        => jsonFileName.FileHandle().String = o.ToJson();

        internal static string PathToFactorioStyle(this string name) =>
            name
                .Replace(Constants.SystemWriteData, Constants.SystemWriteDataPlaceholder)
                .Replace("\\", "/");

        internal static string PathFromFactorioStyle(this string name) =>
            name
                .Replace(Constants.SystemWriteDataPlaceholder, Constants.SystemWriteData)
                .Replace("/", "\\");
    }
}