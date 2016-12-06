using System;
using System.Collections.Generic;
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
        const double TicksPerSecond = 60.0;

        readonly string Path;
        readonly MmasfContext Parent;

        Version VersionValue;
        ModDescription[] ModsValue;
        string MapName;
        string ScenarioName;
        string CampaignName;
        TimeSpan? DurationValue;
        public Resource[] Resources;
        public byte[] BeforeMods;
        public SomeStruct[] Structs;

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
            CampaignName.Quote() + "  " + 
            Duration.Format3Digits();

        public Version Version
        {
            get
            {
                if(VersionValue == null)
                    ReadLevelDatFile();
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

        public ModDescription[] Mods
        {
            get
            {
                if(ModsValue == null)
                    ReadLevelDatFile();
                return ModsValue;
            }
        }

        void ReadLevelDatFile()
        {
            var reader = LevelDatReader;

            VersionValue = new Version
            (
                reader.GetNext<short>(),
                reader.GetNext<short>(),
                reader.GetNext<short>(),
                reader.GetNext<short>()
            );

            var isBefore0_13 = Version < new Version(0, 13);
            var is0_13_9_2 = Version == new Version(0, 13, 9, 2);
            var isBefore0_14_9_1 = Version < new Version(0, 14, 9, 1);
            var isBefore0_14_14 = Version < new Version(0, 14, 14);

            ScenarioName = reader.GetNextString<int>();
            CampaignName = reader.GetNextString<int>();
            MapName = reader.GetNextString<int>();

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

            var byte1 = reader.GetNext<byte>();
            if(isBefore0_13)
            {
                var structCount = reader.GetNext<int>();
                Tracer.Assert(structCount < 100);
                Structs = structCount.Select(i => GetStruct(reader)).ToArray();
            }

            var modCount = reader.GetNext<int>();
            Tracer.Assert(modCount < 100);

            ModsValue =
                modCount
                    .Select(i => Parent.CreateModReference(i, reader, isBefore0_14_14))
                    .ToArray();

            var lookAhead = reader.GetBytes(100);
            string someText, someText2;
            if (!isBefore0_13)
                someText = reader.GetNextString<int>();
            if (!isBefore0_14_9_1)
                someText2 = reader.GetNextString<int>();

            DurationValue = TimeSpan.FromSeconds(reader.GetNext<int>() / TicksPerSecond);
            var someBytes3 = reader.GetNextBytes(2);

            var count = reader.GetNext<int>();
            Tracer.Assert(count < 100);

            Resources =
                count
                    .Select(i => GetNextRecource(reader))
                    .ToArray();
        }


        static SomeStruct GetStruct(BinaryRead reader)
        {
            var result = new SomeStruct
            {
                Bytes =
                    reader.GetNextBytes(9)
            };

            var count = reader.GetNext<int>();
            Tracer.Assert(count < 100);
            result.SomeBytes = count.Select
                (
                    i => new SomeStruct.Sub
                    {
                        ShortNumber = reader.GetNext<short>(),
                        Number = reader.GetNext<int>()
                    })
                .ToArray();
            return result;
        }


        [DisableDump]
        public BinaryRead LevelInitDatReader => GetFile(LevelInitDat).BinaryReader;
        [DisableDump]
        public BinaryRead LevelDatReader => GetFile(LevelDat).BinaryReader;

        public sealed class SomeStruct
        {
            public class Sub
            {
                public short ShortNumber;
                public int Number;
            }

            public byte[] Bytes;

            public Sub[] SomeBytes;
        }

        public sealed class Resource
        {
            public string Text;
            public byte[] Number;

            public override string ToString() => Text.Quote() + "(" + Number.Stringify(",") + ")";
        }

        static Resource GetNextRecource(BinaryRead reader)
            => new Resource
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