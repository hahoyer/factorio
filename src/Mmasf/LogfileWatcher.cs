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
        readonly SmbFile Logfile;
        readonly FileSystemWatcher Watcher;
        DateTime? StartTime;
        readonly object Mutex = new object();

        internal LogfileWatcher(SmbFile path)
        {
            Logfile = path.PathCombine(UserConfiguration.LogfileName);
            Watcher = CreateWatcher(path);

            Timer = new Timer(OnTimer);
            Timer.Change(TimeSpan.Zero, 1.Seconds());
        }

        void OnTimer(object state)
        {
            lock(Mutex)
            {
                Tracer.Assert(state == Timer);
                var delta = Logfile.Size - LastSize;

                if (delta == 0)
                    return;

                Tracer.Assert(delta < int.MaxValue);
                Tracer.Assert(delta > 0);

                var newData = Logfile.SubString(LastSize, (int)delta);
                var rawLines = (LastLinePart + newData).Split('\n');
                var lines = rawLines.Take(rawLines.Length - 1).Select(ScanLine).ToArray();

                if (StartTime == null && lines.Any())
                    StartTime = DateTime.Parse(lines[0].Data.Substring(0, 19), CultureInfo.InvariantCulture);

                if (StartTime == null)
                    (Logfile + " " + delta + "\n" + newData).WriteFlaggedLine();
                else
                {
                    var formattedData = lines.Select(l => l.Format(StartTime.Value)).Stringify("\n");
                    (Logfile + " " + delta + "\n" + formattedData).WriteFlaggedLine();
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

        FileSystemWatcher CreateWatcher(SmbFile path)
        {
            var result = new FileSystemWatcher(path.FullName, UserConfiguration.PreviousLogfileName)
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