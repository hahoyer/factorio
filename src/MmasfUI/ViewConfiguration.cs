using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    sealed class ViewConfiguration : DumpableObject
    {
        internal enum TypeEnum
        {
            Saves,
            Mods
        }

        internal readonly string Name;
        internal readonly TypeEnum Type;
        readonly ValueCache<Persister> PersisterCache;
        readonly ValueCache<Window> ViewCache;

        public ViewConfiguration(string name, TypeEnum type)
        {
            Name = name;
            Type = type;
            PersisterCache = new ValueCache<Persister>
                (() => new Persister(ItemFile("View")));
            ViewCache = new ValueCache<Window>(CreateAndConnectView);
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

        internal Window View => ViewCache.Value;

        Window CreateAndConnectView()
        {
            var result = CreateView();
            ConnectToWindow(result);
            return result;
        }

        Window CreateView()
        {
            switch(Type)
            {
                case TypeEnum.Saves:
                    return new SavesWindow(this);
                case TypeEnum.Mods:
                    return new ModsWindow(this);
            }

            return null;
        }

        internal void ConnectToWindow(Window window)
        {
            Status = "Open";
            window.Closing += (a, s) => OnClosing();
            window.Activated += (a, s) => OnActivated();
        }

        Persister ViewPersister => PersisterCache.Value;

        File ItemFile(string itemName) => ItemFileName(itemName).FileHandle();

        string ItemFileName(string itemName)
            => SystemConfiguration
                .GetConfigurationPath(Name + "." + Type)
                .PathCombine(itemName);

        void OnClosing()
        {
            Status = "Closed";
            ViewCache.IsValid = false;
        }

        void OnActivated() { LastUsed = DateTime.Now; }

        internal string PositionPath => ItemFileName("Position");
    }
}