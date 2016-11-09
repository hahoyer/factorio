using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class ModFile : UserConfiguration.INameProvider
    {
        internal readonly string ModName;
        readonly string Version;
        readonly int ConfigGroupIndex;
        readonly string ConfigName;

        public static ModFile Create(string path, string[] prefixes)
        {
            var name = path.FileHandle().Name;
            var nameParts = name.Split('_');
            var modName = nameParts[0];
            var version = nameParts.Length > 1 ? name.Substring(modName.Length + 1) : "<unknown>";

            var dictionary = path
                .FileHandle().DirectoryName
                .FileHandle().DirectoryName;
            var configGroupDirectoryCandidate = dictionary
                .FileHandle().DirectoryName;

            var index = prefixes.IndexWhere(configGroupDirectoryCandidate.StartsWith).AssertValue();
            var configName = dictionary.Substring(prefixes[index].Length + 1);


            return new ModFile(modName, version, index, configName);
        }

        ModFile(string modName, string version, int configGroupIndex, string configName)
        {
            ModName = modName;
            Version = version;
            ConfigGroupIndex = configGroupIndex;
            ConfigName = configName;
        }

        string UserConfiguration.INameProvider.Name => ModName;

        public override string ToString()
            => ConfigGroupIndex +
            "." +
            ConfigName +
            ":" +
            ModName +
            " " +
            Version;
    }
}