using System;
using System.IO;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class SavefileWatcher : DumpableObject
    {
        readonly FileSystemWatcher Watcher;

        SavefileWatcher(FileSystemWatcher watcher) { Watcher = watcher; }

        internal static SavefileWatcher Create(string path)
        {
            var m = new FileSystemWatcher(path.PathCombine(UserConfiguration.SaveDirectoryName))
            {
                NotifyFilter = NotifyFilters.CreationTime
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            m.Changed += OnLogfileAppend;
            m.Created += OnLogfileAppend;
            m.Deleted += OnLogfileAppend;
            m.Renamed += OnLogfileAppend;
            return new SavefileWatcher(m);
        }

        static void OnLogfileAppend(object sender, FileSystemEventArgs e)
        {
            (DateTime.Now.DynamicShortFormat(showMiliseconds: true) + " " + e.FullPath)
                .WriteLine();
        }
    }
}