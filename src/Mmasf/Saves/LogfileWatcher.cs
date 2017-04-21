using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    sealed class LogfileWatcher : DumpableObject
    {
        long LastSize;
        readonly Timer Timer;
        readonly string LogfileName;
        readonly FileSystemWatcher Watcher;

        internal LogfileWatcher(string path)
        {
            LogfileName = path.PathCombine(UserConfiguration.LogfileName);
            Watcher = CreateWatcher(path);

            Timer = new Timer(OnTimer);
            Timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        void OnTimer(object state)
        {
            Tracer.Assert(state == Timer);
            var fileHandle = LogfileName.ToSmbFile();
            var size = fileHandle.Size;
            if(LastSize == size)
                return;

            var delta = size - LastSize;
            Tracer.Assert(delta < int.MaxValue);
            var newData = fileHandle.SubString(LastSize, (int) delta);
            (LogfileName + " " + delta + "\n" + newData).WriteFlaggedLine();

            LastSize = size;
        }

        FileSystemWatcher CreateWatcher(string path)
        {
            var result = new FileSystemWatcher(path, UserConfiguration.PreviousLogfileName)
            {
                NotifyFilter = NotifyFilters.CreationTime
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            result.Created += OnLogfileCreated;
            result.Changed += OnLogfileCreated;
            return result;
        }

        void OnLogfileCreated(object sender, FileSystemEventArgs e)
        {
            (DateTime.Now.DynamicShortFormat(true) + " " + e.FullPath)
                .WriteLine();
            LastSize = 0;
        }
    }
}