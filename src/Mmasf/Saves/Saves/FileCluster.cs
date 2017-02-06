using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Reader;

namespace ManageModsAndSavefiles.Saves
{
    public sealed class FileCluster : DumpableObject
    {
        sealed class VersionReader : DumpableObject, BinaryRead.IReaderProvider
        {
            object BinaryRead.IReaderProvider.ReadAndAdvance
                (BinaryRead reader, Type type, MemberInfo member)
                => new Version
                (
                    reader.GetNext<short>(),
                    reader.GetNext<short>(),
                    reader.GetNext<short>(),
                    reader.GetNext<short>()
                );
        }

        sealed class ExactVersionReader : DumpableObject, BinaryRead.IReaderProvider
        {
            object BinaryRead.IReaderProvider.ReadAndAdvance
                (BinaryRead reader, Type type, MemberInfo member)
            {
                var isBefore01414 = ((UserContext) reader.UserContext).IsBefore01414;

                return isBefore01414
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
            }
        }

        sealed class ReaderBefore013 : DumpableObject, BinaryRead.IReaderProvider
        {
            object BinaryRead.IReaderProvider.ReadAndAdvance(BinaryRead reader, Type type, MemberInfo member)
                => ((UserContext) reader.UserContext).IsBefore013
                    ? reader.GetNext(type, member)
                    : null;
        }

        sealed class BeforeDurationReader : DumpableObject, BinaryRead.IReaderProvider
        {
            object BinaryRead.IReaderProvider.ReadAndAdvance(BinaryRead reader, Type type, MemberInfo member)
            {
                var isBefore0_13 = ((UserContext) reader.UserContext).IsBefore013;
                var isBefore0_14_9_1 = ((UserContext) reader.UserContext).IsBefore01491;

                if(!isBefore0_13)
                    reader.GetNextString<int>();
                if(!isBefore0_14_9_1)
                    reader.GetNextString<int>();

                return TimeSpan.FromSeconds(reader.GetNext<int>() / TicksPerSecond);
            }
        }


        const string LevelInitDat = "level-init.dat";
        const string LevelDat = "level.dat";
        const double TicksPerSecond = 60.0;

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
        protected override string GetNodeDump() => Name;

        public override string ToString()
            => Name.Quote() + "  " +
               Version + "  " +
               DataValue.MapName.Quote() + "  " +
               DataValue.ScenarioName.Quote() + "  " +
               DataValue.CampaignName.Quote() + "  " +
               DataValue.Difficulty + "  " +
               Duration.Format3Digits();

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

        sealed class BinaryData
        {
            [DataItem(CaptureIdentifier = "Version", Reader = typeof(VersionReader))]
            internal Version Version;

            [DataItem]
            internal string ScenarioName;

            [DataItem]
            internal string CampaignName;

            [DataItem]
            internal string MapName;

            [DataItem]
            internal byte Difficulty;

            [Ignore(9)]
            [DataItem(Reader = typeof(ExactVersionReader))]
            internal Version ExactVersion;

            [Ignore(1)]
            [DataItem(Reader = typeof(ReaderBefore013))]
            [ArrayItem]
            internal SomeStruct[] Structs;

            [DataItem]
            [ArrayItem(Reader = typeof(ModDescription.ModsReader))]
            internal ModDescription[] Mods;

            [DataItem(Reader = typeof(BeforeDurationReader))]
            internal TimeSpan Duration;

            [Ignore(2)]
            [DataItem]
            internal Resource[] Resources;
        }

        void EnsureDataRead()
        {
            if(DataValue != null)
                return;

            var reader = Profiler.Measure(() => LevelDatReader);
            reader.UserContext = new UserContext();
            DataValue = reader.GetNext<BinaryData>();
        }

        [DisableDump]
        internal BinaryRead LevelInitDatReader => BinaryRead(LevelInitDat);

        BinaryRead BinaryRead(string fileName)
        {
            var handle = GetFile(fileName);
            var reader = handle.Reader;
            var result = new BinaryRead(reader);
            return result;
        }

        [DisableDump]
        public BinaryRead LevelDatReader => BinaryRead(LevelDat);

        public sealed class SomeStruct
        {
            public sealed class Sub
            {
                [DataItem]
                public short ShortNumber;
                [DataItem]
                public int Number;
            }

            [DataItem]
            [ArrayItem(9)]
            public byte[] Bytes;

            [DataItem]
            [ArrayItem(MaxCount = 100)]
            public Sub[] SomeBytes;
        }

        public sealed class Resource
        {
            [DataItem]
            [ArrayItem(MaxCount = 100)]
            public string Text;
            [DataItem]
            [ArrayItem(3)]
            public byte[] Numbers;

            public override string ToString() => Text.Quote() + "(" + Numbers.Stringify(",") + ")";
        }

        ZipFileHandle GetFile(string name)
        {
            var fileHandle = Profiler.Measure(() => Path.ZipHandle());
            var zipFileHandles = Profiler.Measure(() => fileHandle.Items);
            var zipFileHandle = Profiler.Measure
                (() => zipFileHandles.Where(item => item.ItemName == name && item.Depth == 2));
            return Profiler.Measure(() => zipFileHandle.Single());
        }

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