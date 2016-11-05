using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class ModFile : UserConfiguration.INameProvider
    {
        readonly string Path;
        public ModFile(string path) { Path = path; }

        string Name => Path.FileHandle().Name;

        string UserConfiguration.INameProvider.Name => Name;
        public override string ToString() => Name.Quote();
    }
}