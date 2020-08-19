using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace ManageModsAndSaveFiles
{
    public sealed class LogfileWatcher : DumpableObject
    {
        readonly List<Line[]> Data = new List<Line[]>();
        long LastSize;
        string LastLinePart = "";

        readonly Timer Timer;
        readonly SmbFile Logfile;
        readonly FileSystemWatcher Watcher;
        DateTime? StartTime;
        readonly object Mutex = new object();

        public LogfileWatcher(SmbFile path)
        {
            Logfile = path.PathCombine(Constants.LogfileName);
            Watcher = CreateWatcher(path);

            Timer = new Timer(OnTimer);
            Timer.Change(TimeSpan.Zero, 1.Seconds());
        }

        void OnTimer(object state)
        {
            lock(Mutex)
            {
                Tracer.Assert(state == Timer);
                ScanNewPart();
            }
        }

        Line[] LinesCache;
        Line[] GetLines() => Data.SelectMany(x => x).Concat(T(ScanLine(LastLinePart))).ToArray();

        public Line[] Lines
        {
            get
            {
                lock(Mutex)
                    return LinesCache ??= GetLines();
            }
        }

        void ScanNewPart()
        {
            var delta = Logfile.Size - LastSize;

            if(delta == 0)
                return;

            Tracer.Assert(delta < int.MaxValue);
            Tracer.Assert(delta > 0);

            var newData = Logfile.SubString(LastSize, (int) delta);
            var target = LastLinePart + newData;
            var rawLines = Recombine(target.Split('\n')).ToArray();

            var start = 0;
            if(StartTime == null && rawLines.Any())
            {
                start = 1;
                StartTime = DateTime.Parse(rawLines[0].Substring(9, 19), CultureInfo.InvariantCulture);
            }

            var lines = rawLines
                .Skip(start)
                .Take(rawLines.Length - 1 - start)
                .Select(ScanLine)
                .ToArray();

            if(StartTime == null)
                (Logfile + " " + delta + "\n" + newData).WriteFlaggedLine();
            else
            {
                LinesCache = null;
                Data.Add(lines);
                var formattedData = lines.Select(l => l.Format(StartTime.Value)).Stringify("\n");
                (Logfile + " " + delta + "\n" + formattedData).WriteFlaggedLine();
            }

            LastSize += delta;
            LastLinePart = rawLines.Last();
        }

        static IEnumerable<string> Recombine(string[] target)
        {
            var index = 0;
            var line = "";
            var result = new List<string>();
            do
            {
                line += target[index++];
                if(index < target.Length && GetTimeStamp(target[index]) == null)
                    line += "\n";
                else
                {
                    result.Add(line);
                    if(index == target.Length)
                        return result;
                    line = "";
                }
            }
            while(index < target.Length);

            result.Add(line);
            return result;
        }

        static decimal? GetTimeStamp(string target)
        {
            if(target.Length < 8)
                return null;
            var timeCodePart = target.Substring(0, 8);
            if(decimal.TryParse(timeCodePart, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return null;
        }

        static Line ScanLine(string text) => new Line {Text = text};

        public sealed class Line
        {
            public decimal Ticks => GetTimeStamp(Text).AssertValue();
            public string Text;
            ILineData LineDataCache;

            public ILineData LineData => LineDataCache ??= GetLineData();

            ILineData GetLineData()
            {
                var tail = Text.Substring(9);

                if(tail.StartsWith(ScriptLine.Head))
                    return new ScriptLine(tail);
                return new UnknownLine(tail);
            }

            public string Format(DateTime startTime)
                => (startTime + TimeSpan.FromSeconds((double) Ticks)).DynamicShortFormat(true) + " " + Text;
        }

        FileSystemWatcher CreateWatcher(SmbFile path)
        {
            var result = new FileSystemWatcher(path.FullName, Constants.PreviousLogfileName)
            {
                NotifyFilter
                    = NotifyFilters.CreationTime |
                      NotifyFilters.LastAccess |
                      NotifyFilters.LastWrite |
                      NotifyFilters.Size,
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

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }

    sealed class UnknownLine : ILineData
    {
        readonly string Line;
        public UnknownLine(string line) => Line = line;
    }

    public sealed class ScriptLine : ILineData
    {
        static readonly Regex Format = new Regex("^__(\\w+)__/([^:]+):(\\d+): (.*)$");
        string ModName;
        string SourceFile;
        int SourceLine;
        string Text;

        public ScriptLine(string line)
        {
            var tail = line.Substring(Head.Length);
            var parts = Format.Match(tail).Groups;
            ModName = parts[1].Value;
            SourceFile = parts[2].Value;
            SourceLine = int.Parse(parts[3].Value);
            Text = parts[4].Value;
        }

        public const string Head = "Script @";
    }

    public interface ILineData {}
}