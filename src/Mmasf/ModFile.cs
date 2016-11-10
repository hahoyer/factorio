using System.Linq;
using System.Runtime.Serialization;
using hw.DebugFormatter;
using hw.Helper;


namespace ManageModsAndSavefiles
{
    sealed class ModFile : DumpableObject, UserConfiguration.INameProvider
    {
        internal readonly int ConfigIndex;
        readonly File File;
        internal readonly string ModName;
        internal readonly string Version;

        ModFile(string path, int configIndex)
        {
            File = path.FileHandle();
            ModName = GetModNameFromFileName();
            Version = GetVersionFromFile();
            ConfigIndex = configIndex;
        }

        string UserConfiguration.INameProvider.Name => ModName;

        public static ModFile Create(string path, string[] paths)
        {
            var dictionary = path
                .FileHandle().DirectoryName
                .FileHandle().DirectoryName;

            var index = paths
                .OrderByDescending(item => item.Split('\\').Length)
                .ThenBy(item => item)
                .IndexWhere(dictionary.StartsWith)
                .AssertValue();

            return new ModFile(path, index);
        }

        string GetVersionFromFileName()
        {
            var nameParts = File.Name.Split('_');
            if(nameParts.Length == 1)
                return "<unknown>";

            return File.Name.Substring(nameParts[0].Length + 1);
        }

        string GetVersionFromFile()
        {
            var text =
                File.IsDirectory ? GetInfoJSonFromDirectory() : GetInfoJSonFromZipFile();
            var info = text.FromJson<ModInfo>();
            return info.Version;
        }

        string GetInfoJSonFromZipFile()
        {
            var headerDir = File.Name.Substring(0, File.Name.Length - 4);
            return File
                .FullName
                .ZipFileHandle()
                .GetItem(headerDir + "/info.json")
                .String;
        }

        string GetInfoJSonFromDirectory()
            => File.FullName.PathCombine("info.json")
                .FileHandle()
                .String;

        string GetModNameFromFileName() => File.Name.Split('_')[0];

        public override string ToString()
            => ConfigIndex + ":" +
               ModName + " " +
               Version;
    }

    class ModInfo
    {
        [DataMember(Name = "author")]
        public string Author;
        [DataMember(Name = "contact")]
        public string Contact;
        [DataMember(Name = "dependencies")]
        public string[] Dependencies;
        [DataMember(Name = "description")]
        public string Description;
        [DataMember(Name = "factorio_version")]
        public string FactorioVersion;
        [DataMember(Name = "homepage")]
        public string Homepage;
        [DataMember(Name = "name")]
        public string Name;
        [DataMember(Name = "title")]
        public string Title;
        [DataMember(Name = "version")]
        public string Version;
    }
}