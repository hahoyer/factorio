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
            object BinaryRead.IReaderProvider.ReadAndAdvance
                (BinaryRead reader, Type type, MemberInfo member)
                => ((UserContext) reader.UserContext).IsBefore013
                    ? reader.GetNext(type, member)
                    : null;
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
            [Ignore(9)]
            internal byte Difficulty;
            [DataItem(Reader = typeof(ExactVersionReader))]
            [Ignore(1)]
            //[Ignore(0, CaptureIdentifier = "Lookahead")]
            internal Version ExactVersion;
            [DataItem(Reader = typeof(ReaderBefore013))]
            [ArraySetup(typeof(int))]
            internal SomeStruct[] Structs;
            [DataItem]
            internal ModDescription[] Mods;

            internal TimeSpan Duration;
            internal Resource[] Resources;
            internal byte[] BeforeMods;
        }

        void EnsureDataRead()
        {
            if(DataValue != null)
                return;

            var reader = LevelDatReader;
            reader.UserContext = new UserContext();

            DataValue = reader.GetNext<BinaryData>();

            var isBefore0_13 = Version < new Version(0, 13);
            var isBefore0_14_9_1 = Version < new Version(0, 14, 9, 1);
            var isBefore0_14_14 = Version < new Version(0, 14, 14);

            var modCount = reader.GetNext<int>();
            Tracer.Assert(modCount < 100);

            DataValue.Mods =
                modCount
                    .Select(i => Parent.CreateModReference(i, reader, isBefore0_14_14))
                    .ToArray();

            string someText, someText2;
            if(!isBefore0_13)
                someText = reader.GetNextString<int>();
            if(!isBefore0_14_9_1)
                someText2 = reader.GetNextString<int>();

            DataValue.Duration = TimeSpan.FromSeconds(reader.GetNext<int>() / TicksPerSecond);
            var someBytes3 = reader.GetNextBytes(2);

            DataValue.Resources = reader.GetNextArray<int, Resource>(100);
        }

        [DisableDump]
        public BinaryRead LevelInitDatReader => BinaryRead(LevelInitDat);

        BinaryRead BinaryRead(string fileName)
            => new BinaryRead(GetFile(fileName).Reader);

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
            [ArraySetup(9)]
            public byte[] Bytes;

            [DataItem]
            [ArraySetup(typeof(int), MaxCount = 100)]
            public Sub[] SomeBytes;
        }

        public sealed class Resource
        {
            [DataItem]
            [ArraySetup(typeof(int), MaxCount = 100)]
            public string Text;
            [DataItem]
            [ArraySetup(3)]
            public byte[] Numbers;

            public override string ToString() => Text.Quote() + "(" + Numbers.Stringify(",") + ")";
        }

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


    sealed class UserContext : DumpableObject, BinaryRead.IContext
    {
        Version Version;
        public bool IsBefore013 => Version < new Version(0, 13);
        public bool Is01392 => Version == new Version(0, 13, 9, 2);
        public bool IsBefore01491 => Version < new Version(0, 14, 9, 1);
        public bool IsBefore01414 => Version < new Version(0, 14, 14);


        void BinaryRead.IContext.Got
            (BinaryRead reader, MemberInfo member, object captureIdentifier, object result)
        {
            if(captureIdentifier as string == "Version")
            {
                Version = (Version) result;
                return;
            }

            if(captureIdentifier as string == "Lookahead")
            {
                var value = reader.GetBytes(100);
                Tracer.TraceBreak();
                return;
            }

            NotImplementedMethod(member.Name, captureIdentifier, result.ToString());
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