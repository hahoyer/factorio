using System;
using System.IO;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSaveFiles;

sealed class SaveFileWatcher : DumpableObject
{
    readonly FileSystemWatcher Watcher;

    SaveFileWatcher(FileSystemWatcher watcher) => Watcher = watcher;

    internal static SaveFileWatcher Create(string path)
    {
        var m = new FileSystemWatcher(path.PathCombine(Constants.SaveDirectoryName))
        {
            NotifyFilter = NotifyFilters.CreationTime |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size
            , EnableRaisingEvents = true
        };
        m.Changed += OnLogfileAppend;
        m.Created += OnLogfileAppend;
        m.Deleted += OnLogfileAppend;
        m.Renamed += OnLogfileAppend;
        return new(m);
    }

    static void OnLogfileAppend
        (object sender, FileSystemEventArgs e) => (DateTime.Now.DynamicShortFormat(true) + " " + e.FullPath)
        .Log();
}