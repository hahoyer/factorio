using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Compression;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Reader;

namespace ManageModsAndSavefiles.Saves
{
    public sealed class FileCluster : DumpableObject
    {
        const string LevelInitDat = "level-init.dat";
        const string LevelDat = "level.dat";

        readonly string Path;
        public readonly UserConfiguration Parent;

        BinaryData DataValue;

        public FileCluster(string path, UserConfiguration parent)
        {
            Path = path;
            Parent = parent;
            Tracer.Line(Path);
        }

        BinaryData Data
        {
            get
            {
                EnsureDataRead();
                return DataValue;
            }
        }

        public string Name => Path.ToSmbFile().Name;
        public DateTime Created => Path.ToSmbFile().ModifiedDate;
        public Version Version => Data.Version;
        public string ScenarioName => Data.ScenarioName;
        public string MapName => Data.MapName;
        public string CampaignName => Data.CampaignName;
        public TimeSpan Duration => Data.Duration;
        public ModDescription[] Mods => Data.Mods;

        public bool IsDataRead
        {
            get { return DataValue != null; }
            set
            {
                if(value)
                    EnsureDataRead();
                else
                    DataValue = null;
            }
        }

        public override string ToString()
            => Name.Quote() + "  " +
                Version + "  " +
                Data.MapName.Quote() + "  " +
                Data.ScenarioName.Quote() + "  " +
                Data.CampaignName.Quote() + "  " +
                Data.Difficulty + "  " +
                Duration.Format3Digits();

        protected override string GetNodeDump() => Name;

        [DisableDump]
        public BinaryRead LevelDatReader => BinaryRead(LevelDat);

        [DisableDump]
        public IEnumerable<ModConflict> Conflicts
            => Mods.Where(m => m.Name != "base")
                .Merge
                (
                    Parent.ModFiles.Where(m => m.IsEnabled == true),
                    arg => arg.Name,
                    arg => arg.Description.Name
                )
                .SelectMany(item => CreateConflict(item.Item2, item.Item3).NullableToArray());

        [DisableDump]
        public IEnumerable<ModConflict> RelevantConflicts
            => Conflicts.Where(c => c.IsRelevant);

        ModConflict CreateConflict(ModDescription saveMod, Mods.FileCluster gameMod)
        {
            if(saveMod?.Version == gameMod?.Version)
                return null;

            return
                new ModConflict
                {
                    Save = this,
                    SaveMod = saveMod,
                    GameMod = gameMod?.Description
                };
        }

        void EnsureDataRead()
        {
            if(IsDataRead)
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

        IZipFileHandle GetFile(string name)
        {
            var fileHandle = Profiler.Measure(() => Path.ZipHandle());
            var zipFileHandles = Profiler.Measure(() => fileHandle.Items);
            var zipFileHandle = Profiler.Measure
                (() => zipFileHandles.Where(item => item.ItemName == name && item.Depth == 2));
            return Profiler.Measure(() => zipFileHandle.Single());
        }
    }
}