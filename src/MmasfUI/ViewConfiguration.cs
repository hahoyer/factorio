using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    public sealed class ViewConfiguration : DumpableObject
    {
        internal readonly string[] Identifier;

        [DisableDump]
        readonly ValueCache<Window> ViewCache;

        internal ViewConfiguration(string[] identifier)
        {
            Identifier = identifier;
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

        [DisableDump]
        internal Window View => ViewCache.Value;

        Window CreateAndConnectView()
        {
            var result = CreateView();
            ConnectToWindow(result);
            return result;
        }

        Window CreateView()
        {
            switch(Identifier[0])
            {
            case "ModDictionary":
                return new ModDictionaryView(this);
            case "Saves":
                return new SavesView(this);
            case "Mods":
                return new ModsView(this);
            case "ModConflicts":
                return new ModConflictsView(this);
            }

            NotImplementedMethod();
            return null;
        }

        internal void ConnectToWindow(Window window)
        {
            Status = "Open";
            window.Closing += (a, s) => OnClosing();
            window.Activated += (a, s) => OnActivated();
        }

        SmbFile ItemFile(string itemName) => ItemFileName(itemName).ToSmbFile();

        string ItemFileName(string itemName)
            => SystemConfiguration
                .GetConfigurationPath(Identifier)
                .PathCombine(itemName);

        void OnClosing()
        {
            if(MainContainer.Instance.IsClosing)
                return;

            Status = "Closed";
            ViewCache.IsValid = false;
            MainContainer.Instance.RemoveViewConfiguration(this);
        }

        void OnActivated()
        {
            LastUsed = DateTime.Now;
            MainContainer.Instance.AddViewConfiguration(this);
        }

        [DisableDump]
        internal string PositionPath => ItemFileName("Position");

        internal void ShowAndActivate()
        {
            View.Show();
            View.Activate();
        }
    }
}