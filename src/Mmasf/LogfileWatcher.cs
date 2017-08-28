using System;
using System.Collections.Generic;
using System.Globalization;
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
        string LastLinePart;
        readonly Timer Timer;
        readonly string LogfileName;
        readonly FileSystemWatcher Watcher;
        DateTime? StartTime;
        readonly object Mutex = new object();

        internal LogfileWatcher(string path)
        {
            LogfileName = path.PathCombine(UserConfiguration.LogfileName);
            Watcher = CreateWatcher(path);

            Timer = new Timer(OnTimer);
            Timer.Change(TimeSpan.Zero, 1.Seconds());
        }

        void OnTimer(object state)
        {
            lock(Mutex)
            {
                Tracer.Assert(state == Timer);
                var fileHandle = LogfileName.ToSmbFile();
                var delta = fileHandle.Size - LastSize;

                if (delta == 0)
                    return;

                Tracer.Assert(delta < int.MaxValue);
                Tracer.Assert(delta > 0);

                var newData = fileHandle.SubString(LastSize, (int)delta);
                var rawLines = (LastLinePart + newData).Split('\n');
                var lines = rawLines.Take(rawLines.Length - 1).Select(ScanLine).ToArray();

                if (StartTime == null && lines.Any())
                    StartTime = DateTime.Parse(lines[0].Data.Substring(0, 19), CultureInfo.InvariantCulture);

                if (StartTime == null)
                    (LogfileName + " " + delta + "\n" + newData).WriteFlaggedLine();
                else
                {
                    var formattedData = lines.Select(l => l.Format(StartTime.Value)).Stringify("\n");
                    (LogfileName + " " + delta + "\n" + formattedData).WriteFlaggedLine();
                }

                LastSize += delta;
                LastLinePart = rawLines.Last();
            }
        }

        static Line ScanLine(string data)
            => new Line
            {
                Ticks = decimal.Parse(data.Substring(0, 8), CultureInfo.InvariantCulture),
                Data = data.Substring(9)
            };

        sealed class Line
        {
            public decimal Ticks;
            public string Data;

            public string Format(DateTime startTime)
                => (startTime + TimeSpan.FromSeconds((double) Ticks)).DynamicShortFormat(true)
                   + " " + Data;
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
            lock(Mutex)
            {
                (DateTime.Now.DynamicShortFormat(true) + " " + e.FullPath)
                    .WriteLine();
                LastSize = 0;
                LastLinePart = "";
                StartTime = null;
            }
        }
    }
}