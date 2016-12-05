using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles.Mods
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
                .GroupBy(item => item.Description.Name)
                .Select(files => GetModLine(UserConfigurations.Length, files))
                .ToArray();
        }

        static ModLine GetModLine(int userConfigurations, IGrouping<string, FileCluster> arg)
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

            public static Cell Create(FileCluster modFile)
                => modFile == null ? new Cell() : new Cell(modFile.Description.Name, modFile.IsEnabled);
        }

        internal static ModLine Create(int userConfigurations, IGrouping<string, FileCluster> arg)
        {
            var modFiles = userConfigurations
                .Select(index => arg.SingleOrDefault(item => item.ConfigIndex == index))
                .ToArray();
            return new ModLine(arg.Key, modFiles);
        }

        Cell[] Cells;

        ModLine(string modName, FileCluster[] modFiles)
        {
            ModName = modName;
            Cells = modFiles.Select(Cell.Create).ToArray();
        }
    }
}