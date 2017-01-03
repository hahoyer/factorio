using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using ManageModsAndSavefiles.Reader;
using ManageModsAndSavefiles.Saves;

namespace ManageModsAndSavefiles.Mods
{
    public sealed class ModDescription : DumpableObject
    {
        public sealed class ModsReader : DumpableObject, BinaryRead.IReaderProvider
        {
            object BinaryRead.IReaderProvider.ReadAndAdvance(BinaryRead reader, Type type, MemberInfo member)
            {
                var x = reader.GetBytes(100);
                var isBefore01414 = ((UserContext) reader.UserContext).IsBefore01414;

                if(isBefore01414)
                {
                    return new ModDescription
                    (
                        reader.GetNextString<int>(),
                        new Version
                        (
                            reader.GetNext<short>(), 
                            reader.GetNext<short>(), 
                            reader.GetNext<short>()
                        )
                    );
                }

                return new ModDescription
                (
                    reader.GetNextString<byte>(),
                    new Version
                    (
                        reader.GetNext<byte>(),
                        reader.GetNext<byte>(),
                        reader.GetNext<byte>()
                    )
                );
            }
        }

        public readonly string Name;
        public readonly Version Version;

        InfoJSon InfoJSonValue;

        public ModDescription(string name, Version version)
        {
            Name = name;
            Version = version;
            Tracer.Assert(!string.IsNullOrEmpty(Name));
        }

        public InfoJSon InfoJSon
        {
            get { return InfoJSonValue; }
            set
            {
                if(InfoJSonValue == null)
                {
                    InfoJSonValue = value;
                    return;
                }

                if(InfoJSonValue == value)
                    return;

                NotImplementedMethod(value);
            }
        }
    }
}