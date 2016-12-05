using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Mods;

namespace ManageModsAndSavefiles.Saves
{
    public sealed class FileCluster : DumpableObject
    {
        const string LevelInitDat = "level-init.dat";
        const string LevelDat = "level.dat";
        const int DurationPosition = 346;
        const double TicksPerSecond = 60.0;

        readonly string Path;
        readonly MmasfContext Parent;

        Version VersionValue;
        ModDescription[] ModsValue;
        string MapName;
        string ScenarioName;
        string CampaignName;
        TimeSpan? DurationValue;
        public Item[] SomeItems;

        public FileCluster(string path, MmasfContext parent)
        {
            Path = path;
            Parent = parent;
        }

        public string Name => Path.FileHandle().Name;
        protected override string GetNodeDump() => Name;

        public override string ToString()
            => Name.Quote() + "  " +
            Version + "  " +
            MapName.Quote() + "  " +
            ScenarioName.Quote() + "  " +
            CampaignName.Quote() + "  ";

        public Version Version
        {
            get
            {
                if(VersionValue == null)
                    ReadLevelInitDatFile();
                return VersionValue;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                if(DurationValue == null)
                    ReadLevelDatFile();
                return DurationValue.AssertValue();
            }
        }

        void ReadLevelDatFile()
        {
            var reader = LevelInitDatReader;
            reader.Position = DurationPosition;
            DurationValue = TimeSpan.FromSeconds(reader.GetNext<int>() / TicksPerSecond);
        }

        public ModDescription[] Mods
        {
            get
            {
                if(ModsValue == null)
                    ReadLevelInitDatFile();
                return ModsValue;
            }
        }

        void ReadLevelInitDatFile()
        {
            var reader = LevelInitDatReader;
            var version = new Version
            (
                reader.GetNext<short>(),
                reader.GetNext<short>(),
                reader.GetNext<short>(),
                reader.GetNext<short>()
            );

            VersionValue = version;
            var campaignName = reader.GetNextString<int>();
            CampaignName = campaignName;
            var mapName = reader.GetNextString<int>();
            MapName = mapName;
            var scenarioName = reader.GetNextString<int>();
            ScenarioName = scenarioName;

            var isBefore0_13 = Version < new Version(0, 13);
            var isBefore0_14_9_1 = Version < new Version(0, 14, 9, 1);
            var isBefore0_14_14 = Version < new Version(0, 14, 14);

            var someBytes = reader.GetNextBytes(10);
            var exactVersion =
                isBefore0_14_14
                    ? new Version
                    (
                        reader.GetNext<short>(),
                        reader.GetNext<short>(),
                        reader.GetNext<short>(),
                        reader.GetNext<short>()
                    )
                    : new Version
                    (
                        reader.GetNext<byte>(),
                        reader.GetNext<byte>(),
                        reader.GetNext<byte>(),
                        reader.GetNext<short>()
                    );

            var someBytes2 = reader.GetNextBytes(isBefore0_13 ? 5 : 1);

            var modCount = reader.GetNext<int>();
            Tracer.Assert(modCount < 100);

            ModsValue =
                modCount
                    .Select(i => Parent.CreateModReference(i, reader, isBefore0_14_14))
                    .ToArray();

            var someBytes3 = reader.GetNextBytes(isBefore0_13 ? 6 : isBefore0_14_9_1 ? 10 : 14);
            var lookAhead = reader.GetBytes(150);

            var count = reader.GetNext<int>();
            Tracer.Assert(count < 100);

            SomeItems =
                count
                    .Select(i => GetNextItem(reader))
                    .ToArray();
        }

        [DisableDump]
        public BinaryRead LevelInitDatReader => GetFile(LevelInitDat).BinaryReader;
        [DisableDump]
        public BinaryRead LevelDatReader => GetFile(LevelDat).BinaryReader;

        public sealed class Item
        {
            public string Text;
            public byte[] Number;

            public override string ToString() => Text.Quote() + "(" + Number.Stringify(",") + ")";
        }

        static Item GetNextItem(BinaryRead reader)
            => new Item
            {
                Text = reader.GetNextString<int>(),
                Number = new[]
                {
                    reader.GetNext<byte>(),
                    reader.GetNext<byte>(),
                    reader.GetNext<byte>()
                }
            };

        ZipFileHandle GetFile(string name)
            => Path
                .ZipFileHandle()
                .Items
                .Single(item => item.ItemName == name);

        public ModConflict GetConflict(ModDescription saveMod, Mods.FileCluster mod)
        {
            if(mod == null)
                return
                    new ModConflict.RemovedMod
                    {
                        Save = this,
                        SaveMod = saveMod
                    };

            if(saveMod == null)
                return new ModConflict.AddedMod
                {
                    Save = this,
                    Mod = mod
                };

            if(saveMod.Version == mod.Description.Version)
                return null;

            return
                new ModConflict.UpdatedMod
                {
                    Save = this,
                    SaveMod = saveMod,
                    ModVersion = mod.Description.Version
                };
        }
    }

    // d4rkpl4y3r

    //int main()
    //{
    //    string file = "C:\\Users\\USER\\Desktop\\level-init.dat";
    //    char* buffer = new char[257];
    //    ifstream is(file, ifstream::binary);
    //is.read(buffer, 48);
    //is.read(buffer, 4);
    //    int modCount = buffer[0];
    //    while (modCount > 0)
    //    {
    //    is.read(buffer, 1);
    //        int length = buffer[0];
    //    is.read(buffer, length);
    //        buffer[length] = 0;
    //        cout << buffer;
    //    is.read(buffer, 3);
    //        cout << " v" << ((int)buffer[0]) << '.' << ((int)buffer[1]) << '.' << ((int)buffer[2]) << endl;
    //        modCount--;
    //    }
    //}
}