using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace ManageModsAndSavefiles.Mods
{
    public sealed class ModDescription : DumpableObject
    {
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