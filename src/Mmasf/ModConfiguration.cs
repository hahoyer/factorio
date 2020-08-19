using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;
using Newtonsoft.Json;

namespace ManageModsAndSaveFiles
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

            internal bool IsEmpty => IsGameOnlyPossible == null && IsSaveOnlyPossible == null;
        }

        internal void Persist()
        {
            Path.ToSmbFile().EnsureDirectoryOfFileExists();
            Path.ToJsonFile(this);
        }

        [DisableDump]
        [JsonIgnore]
        public bool IsDirty
        {
            get
            {
                if(!Path.ToSmbFile().Exists)
                    return Data.Any();

                var internalVersion = this.ToJson();
                var persistentVersion = Path.ToSmbFile().String;
                return internalVersion != persistentVersion;
            }
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

        public void Save()
        {
            if(IsDirty)
                Persist();
        }
    }
}