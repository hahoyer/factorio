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
        internal const string SavesType = "Saves";

        internal readonly string FileName;
        readonly ValueCache<Persister> PersisterCache;

        public FileConfiguration(string fileName)
        {
            FileName = fileName;
            PersisterCache = new ValueCache<Persister>
                (() => new Persister(ItemFile("View")));
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

        internal string Type
        {
            get { return ItemFile("Type").String; }
            private set { ItemFile("Type").String = value; }
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

        internal Window CreateView()
        {
            Persister.Register("Type", OnLoadType, OnSaveType);
            Persister.Load();

            if(Type == null)
                Type = SavesType;

            var result = new UserConfigurationWindow(this);
            ConnectToWindow(result);
            return result;
        }

        internal void ConnectToWindow(Window window)
        {
            Status = "Open";
            window.Closing += (a, s) => OnClosing();
            window.Activated += (a, s) => OnActivated();
        }

        string OnSaveType()
        {
            NotImplementedMethod();
            return null;
        }

        void OnLoadType(string obj) { NotImplementedMethod(obj); }

        Persister Persister => PersisterCache.Value;

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