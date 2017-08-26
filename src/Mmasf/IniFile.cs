using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using IniParser.Model;

namespace ManageModsAndSavefiles
{
    sealed class IniFile
    {
        private string CommentString ;
        readonly ValueCache<IniData> Data;
        internal readonly string Path;

        internal IniFile(string path, string commentString)
        {
            Path = path;
            CommentString = commentString;
            Data = new ValueCache<IniData>(() => Path.FromIni(CommentString));
        }

        internal void Persist() => Data.Value.SaveTo(Path, CommentString);

        internal KeyDataCollection this[string name] => Data.Value[name];
        internal KeyDataCollection Global => Data.Value.Global;

        internal void UpdateFrom(IniFile source)
        {
            var destinationFile = Path.ToSmbFile();
            var sourceFile = source.Path.ToSmbFile();
            if (!destinationFile.Exists || destinationFile.ModifiedDate < sourceFile.ModifiedDate)
            {
                destinationFile.EnsureDirectoryOfFileExists();
                destinationFile.String = sourceFile.String;
            }

            Data.IsValid = false;
        }
    }
}