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
            object BinaryRead.IReaderProvider.ReadAndAdvance
                (BinaryRead reader, Type type, MemberInfo member)
            {
                var x = reader.GetBytes(100);
                var isBefore01414 = ((UserContext) reader.UserContext).IsBefore01414;

                if(isBefore01414)
                    return MmasfContext.Instance.GetModDescription
                    (
                        reader.GetNextString<int>(),
                        new Version
                        (
                            reader.GetNext<short>(),
                            reader.GetNext<short>(),
                            reader.GetNext<short>()
                        ));

                return MmasfContext.Instance.GetModDescription
                (
                    reader.GetNextString<byte>(),
                    new Version
                    (
                        reader.GetNext<byte>(),
                        reader.GetNext<byte>(),
                        reader.GetNext<byte>()
                    ));
            }
        }

        internal ModConfiguration Configuration;

        public readonly string Name;
        public readonly Version Version;
        public string FullName => Name + " " + Version;

        public bool HasConfiguration
        {
            get { return ConfigurationItem != null; }
            set
            {
                if (HasConfiguration == value)
                    return;

                if(value)
                    Configuration.Add(Name);
                else
                    Configuration.Remove(Name);
            }
        }

        ModConfiguration.Item ConfigurationItem
            => Configuration.Data.SingleOrDefault(data => data.Name == Name);

        InfoJSon InfoJSonValue;

        public ModDescription(string name, Version version, ModConfiguration value)
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

        public bool IsCompatible(Version saveModVersion) => true;
    }
}