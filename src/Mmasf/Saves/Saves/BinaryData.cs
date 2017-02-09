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
    sealed class BinaryData
    {
        const double TicksPerSecond = 60.0;

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
                var isBefore01414 = ((UserContext)reader.UserContext).IsBefore01414;

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
                => ((UserContext)reader.UserContext).IsBefore013
                    ? reader.GetNext(type, member)
                    : null;
        }

        sealed class BeforeDurationReader : DumpableObject, BinaryRead.IReaderProvider
        {
            object BinaryRead.IReaderProvider.ReadAndAdvance(BinaryRead reader, Type type, MemberInfo member)
            {
                var isBefore0_13 = ((UserContext)reader.UserContext).IsBefore013;
                var isBefore0_14_9_1 = ((UserContext)reader.UserContext).IsBefore01491;

                if (!isBefore0_13)
                    reader.GetNextString<int>();
                if (!isBefore0_14_9_1)
                    reader.GetNextString<int>();

                return TimeSpan.FromSeconds(reader.GetNext<int>() / TicksPerSecond);
            }
        }

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


        [DataItem(CaptureIdentifier = "Version", Reader = typeof(BinaryData.VersionReader))]
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
        [DataItem(Reader = typeof(BinaryData.ExactVersionReader))]
        internal Version ExactVersion;

        [Ignore(1)]
        [DataItem(Reader = typeof(BinaryData.ReaderBefore013))]
        [ArrayItem]
        internal BinaryData.SomeStruct[] Structs;

        [DataItem]
        [ArrayItem(Reader = typeof(ModDescription.ModsReader))]
        internal ModDescription[] Mods;

        [DataItem(Reader = typeof(BinaryData.BeforeDurationReader))]
        internal TimeSpan Duration;

        [Ignore(2)]
        [DataItem]
        internal BinaryData.Resource[] Resources;
    }
}