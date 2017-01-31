using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    sealed class FileConfiguration : DumpableObject
    {
        internal readonly string FileName;
        readonly ValueCache<Persister> PersisterCache;

        public FileConfiguration(string fileName)
        {
            FileName = fileName;
            PersisterCache = new ValueCache<Persister>
                (() => new Persister(ItemFile("EditorConfiguration")));
        }

        internal string Status
        {
            get { return ItemFile("Status").String; }
            private set { ItemFile("Status").String = value; }
        }

        internal DateTime? LastUsed
        {
            get { return FromDateTime(ItemFile("LastUsed").String); }
            private set { ItemFile("LastUsed").String = value?.ToString("O"); }
        }

        static DateTime? FromDateTime(string value)
        {
            if(value == null)
                return null;

            DateTime result;
            if(DateTime.TryParse(value, out result))
                return result;

            return null;
        }

        public Window CreateView(MainContainer parent)
        {
            NotImplementedMethod(parent);
            return null;
        }

        internal void ConnectToEditor(Window editor)
        {
            Persister.Load();
            Persister.Register("Type", OnLoadType, OnSaveType);
            Status = "Open";
        }

        string OnSaveType()
        {
            NotImplementedMethod();
            return null;
        }

        void OnLoadType(string obj) { NotImplementedMethod(obj); }

        Persister Persister => PersisterCache.Value;

        internal void ConnectToFrame(Window frame)
        {
            frame.Closing += (a, s) => OnClosing();
            frame.Activated += (a, s) => OnActivated();
        }

        File ItemFile(string itemName) => ItemFileName(itemName).FileHandle();

        string ItemFileName(string itemName)
            => SystemConfiguration
                .GetConfigurationPath(FileName)
                .PathCombine(itemName);

        void OnClosing() { Status = "Closed"; }
        void OnActivated() { LastUsed = DateTime.Now; }

        internal string PositionPath => ItemFileName("Position");
    }
}