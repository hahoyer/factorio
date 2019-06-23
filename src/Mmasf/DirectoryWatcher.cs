using System;
using System.IO;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;

namespace ManageModsAndSaveFiles
{
    sealed class DirectoryWatcher
    {
        public Action OnExternalModification;
        readonly string[] Exceptions;

        [UsedImplicitly]
        readonly FileSystemWatcher[] Watchers;

        internal DirectoryWatcher(string[] paths, string[] exceptions)
        {
            Exceptions = exceptions;
            Watchers = paths.Select(CreateFileWatcher).ToArray();
        }

        FileSystemWatcher CreateFileWatcher(string path)
        {
            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.CreationTime |
                               NotifyFilters.LastAccess |
                               NotifyFilters.LastWrite |
                               NotifyFilters.Size |
                               NotifyFilters.DirectoryName
            };
            //watcher.Changed += WatcherOnChanged;
            watcher.Created += WatcherOnChanged;
            watcher.Deleted += WatcherOnChanged;
            watcher.Renamed += WatcherOnChanged;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            var watcher = (FileSystemWatcher) sender;
            var root = watcher.Path.ToSmbFile();
            var currentFile = args.FullPath.ToSmbFile();
            if(currentFile.ContainsAnyPattern(root, Exceptions))
                return;
            OnExternalModification?.Invoke();
        }
    }
}