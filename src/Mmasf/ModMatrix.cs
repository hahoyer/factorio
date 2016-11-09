using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class ModMatrix : DumpableObject
    {
        readonly string[] Paths;
        readonly ValueCache<string[]> PrefixesCache;

        internal ModMatrix(string[] paths)
        {
            Paths = paths;
            PrefixesCache = new ValueCache<string[]>(GetPrefixes);

            Test();
        }

        string[] Prefixes => PrefixesCache.Value;

        string[] GetPrefixes() => Paths
            .Select(item => item.FileHandle().DirectoryName)
            .Distinct()
            .OrderByDescending(item => item.Split('\\').Length)
            .ThenBy(item => item)
            .ToArray();

        void Test()
        {
            var ppp = Prefixes;
            var v = Paths
                .SelectMany
                (
                    p => p
                        .PathCombine(UserConfiguration.ModDirectoryName)
                        .FileHandle()
                        .Items
                        .Where(item => item.IsDirectory || item.Extension.ToLower() == ".zip")
                        .Select(item => ModFile.Create(item.FullName, Prefixes))
                )
                .GroupBy(item => item.ModName)
                .ToArray();
        }
    }
}