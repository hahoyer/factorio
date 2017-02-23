using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class ModConfiguration
    {
        const string FileNameEnd = "FactorioMmasf\\modconfig.json";

        static readonly string Path
            = Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine(FileNameEnd);

        internal static ModConfiguration Create()
        {
            var result = Path.FromJsonFile<ModConfiguration>()
                ?? new ModConfiguration();


            result.Persist();
            return result;
        }

        public readonly List<Item> Data = new List<Item>();

        public sealed class Item
        {
            public string Name;
            public bool? IsGameOnlyPossible;
            public bool? IsSaveOnlyPossible;
        }

        void Persist()
        {
            Path.FileHandle().EnsureDirectoryOfFileExists();
            Path.ToJsonFile(this);
        }

        internal void Add(string name)
            => Data.Add
            (
                new Item
                {
                    Name = name
                }
            );

        internal void Remove(string name) => Data.Remove(Data.Single(i => i.Name == name));
    }
}