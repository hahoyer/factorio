using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Compression.Nuget;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Reader;

namespace ManageModsAndSavefiles.Saves
{
    public sealed class FileCluster : DumpableObject
    {
        const string LevelInitDat = "level-init.dat";
        const string LevelDat = "level.dat";

        readonly string Path;
        readonly MmasfContext Parent;

        BinaryData DataValue;

        public FileCluster(string path, MmasfContext parent)
        {
            Path = path;
            Parent = parent;
            Tracer.Line(Path);
        }

        public string Name => Path.FileHandle().Name;
        public DateTime Created => Path.FileHandle().ModifiedDate;

        public Version Version
        {
            get
            {
                EnsureDataRead();
                return DataValue.Version;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                EnsureDataRead();
                return DataValue.Duration;
            }
        }

        public ModDescription[] Mods
        {
            get
            {
                EnsureDataRead();
                return DataValue.Mods;
            }
        }

        public override string ToString()
            => Name.Quote() + "  " +
               Version + "  " +
               DataValue.MapName.Quote() + "  " +
               DataValue.ScenarioName.Quote() + "  " +
               DataValue.CampaignName.Quote() + "  " +
               DataValue.Difficulty + "  " +
               Duration.Format3Digits();

        protected override string GetNodeDump() => Name;

        [DisableDump]
        public BinaryRead LevelDatReader => BinaryRead(LevelDat);

        public ModConflict GetConflict(ModDescription saveMod, Mods.FileCluster mod)
        {
            if (mod == null)
                return
                    new ModConflict.RemovedMod
                    {
                        Save = this,
                        SaveMod = saveMod
                    };

            if (saveMod == null)
                return new ModConflict.AddedMod
                {
                    Save = this,
                    Mod = mod
                };

            if (saveMod.Version == mod.Description.Version)
                return null;

            return
                new ModConflict.UpdatedMod
                {
                    Save = this,
                    SaveMod = saveMod,
                    ModVersion = mod.Description.Version
                };
        }

        void EnsureDataRead()
        {
            if(DataValue != null)
                return;

            var reader = Profiler.Measure(() => LevelDatReader);
            reader.UserContext = new UserContext();
            DataValue = reader.GetNext<BinaryData>();
        }

        BinaryRead BinaryRead(string fileName)
        {
            var handle = GetFile(fileName);
            var reader = handle.Reader;
            var result = new BinaryRead(reader);
            return result;
        }

        ZipFileHandle GetFile(string name)
        {
            var fileHandle = Profiler.Measure(() => Path.ZipHandle());
            var zipFileHandles = Profiler.Measure(() => fileHandle.Items);
            var zipFileHandle = Profiler.Measure
                (() => zipFileHandles.Where(item => item.ItemName == name && item.Depth == 2));
            return Profiler.Measure(() => zipFileHandle.Single());
        }
    }
}