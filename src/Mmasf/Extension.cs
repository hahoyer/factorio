﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using IniParser;
using IniParser.Model;
using ManageModsAndSaveFiles.Compression;
using ManageModsAndSaveFiles.Compression.Microsoft;

namespace ManageModsAndSaveFiles;

public static class Extension
{
    internal const string SystemReadDataPlaceholder = "__PATH__system-read-data__";

    const string SystemWriteDataPlaceholder = "__PATH__system-write-data__";

    internal static readonly SmbFile SystemWriteDataDir
        = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .PathCombine("Factorio")
            .ToSmbFile();

    internal static IEnumerable<SmbFile> FindFilesThatEndsWith
        (this SmbFile root, string target)
        => new[] { root }.FindFilesThatEndsWith(target);

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
                ? new ZipArchiveHandle(name)
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

        return default;
    }

    internal static bool ContainsAnyPattern(this SmbFile file, SmbFile root, string[] patterns)
    {
        var name = file.FullName.Substring(root.FullName.Length);
        return patterns.Any(exception => name.Matches(exception));
    }
}