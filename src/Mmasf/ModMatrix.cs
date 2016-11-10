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

        internal ModMatrix(string[] paths)
        {
            Paths = paths;
            Test();
        }

        string[] GetPrefixes() => Paths
            .Select(item => item.FileHandle().DirectoryName)
            .Distinct()
            .OrderByDescending(item => item.Split('\\').Length)
            .ThenBy(item => item)
            .ToArray();

        void Test()
        {
            var v = Paths
                .SelectMany
                (
                    p => p
                        .PathCombine(UserConfiguration.ModDirectoryName)
                        .FileHandle()
                        .Items
                        .Where(item => item.IsDirectory || item.Extension.ToLower() == ".zip")
                        .Select(item => ModFile.Create(item.FullName, Paths))
                )
                .GroupBy(item => item.ModName)
                .Select(GetModLine)
                .ToArray();
        }

        static ModLine GetModLine(IGrouping<string, ModFile> arg)
            => ModLine.Create(arg);
    }

    sealed class ModLine : DumpableObject
    {
        internal static ModLine Create(IGrouping<string, ModFile> arg)
        {
            var modFiles = arg.OrderBy(item => item.ConfigIndex).ToArray();
            return new ModLine(arg.Key, modFiles);
        }

        ModLine(string modName, ModFile[] modFiles)
        {
            NotImplementedMethod(modName, modFiles.Stringify(","));
        }

    }
}