using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class ModMatrix : DumpableObject
    {
        readonly UserConfiguration[] UserConfigurations;
        public readonly ModLine[] ModLines;

        internal ModMatrix(UserConfiguration[] userConfigurations)
        {
            UserConfigurations = userConfigurations;
            ModLines = UserConfigurations
                .SelectMany(p => p.ModFiles)
                .GroupBy(item => item.ModName)
                .Select(files => GetModLine(UserConfigurations.Length, files))
                .ToArray();
        }

        static ModLine GetModLine(int userConfigurations, IGrouping<string, ModFile> arg)
            => ModLine.Create(userConfigurations, arg);
    }

    public sealed class ModLine : DumpableObject
    {
        readonly string ModName;

        internal sealed class Cell : DumpableObject
        {
            internal readonly string Version;
            internal readonly bool? IsEnabled;

            Cell(string version, bool? isEnabled)
            {
                Version = version;
                IsEnabled = isEnabled;
            }

            Cell() { }

            public static Cell Create(ModFile arg)
                => arg == null ? new Cell() : new Cell(arg.Version, arg.IsEnabled);
        }

        internal static ModLine Create(int userConfigurations, IGrouping<string, ModFile> arg)
        {
            var modFiles = userConfigurations
                .Select(index => arg.SingleOrDefault(item => item.ConfigIndex == index))
                .ToArray();
            return new ModLine(arg.Key, modFiles);
        }

        Cell[] Cells;

        ModLine(string modName, ModFile[] modFiles)
        {
            ModName = modName;
            Cells = modFiles.Select(Cell.Create).ToArray();
        }
    }
}