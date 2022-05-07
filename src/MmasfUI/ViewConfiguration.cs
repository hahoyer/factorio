using System;
using System.Windows;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    public sealed class ViewConfiguration : DumpableObject
    {
        internal interface IWindow
        {
            Window Window { get; }
            void Refresh();
        }

        internal readonly string[] Identifier;

        [DisableDump]
        readonly ValueCache<IWindow> ViewCache;

        internal ViewConfiguration(string[] identifier)
        {
            Identifier = identifier;
            ViewCache = new ValueCache<IWindow>(CreateAndConnectView);
        }

        internal string Status
        {
            get => ItemFile("Status").String;
            private set => ItemFile("Status").String = value;
        }

        internal DateTime? LastUsed
        {
            get => FromDateTime(ItemFile("LastUsed").String);
            private set => ItemFile("LastUsed").String = value?.ToString("O");
        }

        [DisableDump]
        internal IWindow View => ViewCache.Value;

        [DisableDump]
        internal string PositionPath => ItemFileName("Position");

        static DateTime? FromDateTime(string value)
        {
            if(value == null)
                return null;

            DateTime result;
            if(DateTime.TryParse(value, out result))
                return result;

            return null;
        }

        IWindow CreateAndConnectView()
        {
            var result = CreateView();
            ConnectToWindow(result.Window);
            return result;
        }

        IWindow CreateView()
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
                default:
                    NotImplementedMethod();
                    return null;
            }
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

        internal void ShowAndActivate()
        {
            View.Window.Show();
            View.Window.Activate();
        }

        internal void Refresh() => View.Refresh();
    }
}