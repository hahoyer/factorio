using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IniParser.Model;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class IniFile
    {
        readonly ValueCache<IniData> Data;
        internal readonly string Path;

        internal IniFile(string path)
        {
            Path = path;
            Data = new ValueCache<IniData>(() => Path.FromIni());
        }

        internal void Persist() => Data.Value.SaveTo(Path);

        internal KeyDataCollection this[string name] => Data.Value[name];
        internal KeyDataCollection Global => Data.Value.Global;

        internal void UpdateFrom(IniFile source)
        {
            var destinationFile = Path.ToSmbFile();
            var sourceFile = source.Path.ToSmbFile();
            if(!destinationFile.Exists || destinationFile.ModifiedDate < sourceFile.ModifiedDate)
            {
                destinationFile.EnsureDirectoryOfFileExists();
                destinationFile.String = sourceFile.String;
            }

            Data.IsValid = false;
        }
    }
}