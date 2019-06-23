using System;
using System.IO;
using hw.Helper;
using IniParser.Model;
using JetBrains.Annotations;

namespace ManageModsAndSaveFiles
{
    sealed class IniFile
    {
        readonly ValueCache<IniData> Data;
        readonly SmbFile Path;
        readonly string CommentString;

        readonly Action OnExternalModification;
        [UsedImplicitly]
        readonly FileSystemWatcher Watcher;


        internal IniFile(SmbFile path, string commentString, Action onExternalModification)
        {
            Path = path;
            CommentString = commentString;
            OnExternalModification = onExternalModification;
            Data = new ValueCache<IniData>(() => Path.FromIni(CommentString));
            Watcher = CreateWatcher(path);
        }

        internal KeyDataCollection this[string name] => Data.Value[name];
        internal KeyDataCollection Global => Data.Value.Global;

        internal void Persist() => Data.Value.SaveTo(Path, CommentString);

        internal void UpdateFrom(IniFile source)
        {
            var destinationFile = Path;
            var sourceFile = source.Path;
            if(!destinationFile.Exists ||
               destinationFile.ModifiedDate < sourceFile.ModifiedDate)
            {
                destinationFile.EnsureDirectoryOfFileExists();
                destinationFile.String = sourceFile.String;
            }

            Data.IsValid = false;
        }

        FileSystemWatcher CreateWatcher(SmbFile path)
        {
            var result = new FileSystemWatcher(path.DirectoryName, path.Name)
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            result.Created += OnModification;
            result.Changed += OnModification;
            return result;
        }

        void OnModification(object sender, FileSystemEventArgs e)
        {
            Data.IsValid = false;
            OnExternalModification?.Invoke();
        }

    }
}