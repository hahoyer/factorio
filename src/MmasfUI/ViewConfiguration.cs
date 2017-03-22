using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;
using hw.Helper;

#region MyRegion
#endregion


namespace MmasfUI
{
    public sealed class ViewConfiguration : DumpableObject
    {
        internal interface IData
        {
            Window CreateView(ViewConfiguration viewConfiguration);
            string Name { get; }
        }

        internal readonly string[] Identifier;
        readonly ValueCache<Window> ViewCache;
        internal static readonly IData Saves = new SavesType();
        internal static readonly IData Mods = new ModsType();
        internal static readonly IData ModDescriptions = new ModDescriptionsType();

        internal static ViewConfiguration CreateViewConfiguration(string[] identifier)
        {
            return new ViewConfiguration(identifier);
        }

        static IData CreateType(string subIdentifier, string identifier)
        {

            switch(identifier)
            {
            case "Saves":
                return new SavesType();
            case "Mods":
                return new ModsType();
            case "ModDescriptions":
                return new ModDescriptionsType();
            case "ModConflicts":
                return new SaveFileClusterProxy.ModConflicts(subIdentifier);
            default:
                NotImplementedFunction(identifier);
                return null;
            }
        }

        sealed class SavesType : DumpableObject, IData
        {
            Window IData.CreateView(ViewConfiguration viewConfiguration)
                => new SavesView(viewConfiguration);

            string IData.Name => "Saves";
        }

        sealed class ModsType : DumpableObject, IData
        {
            Window IData.CreateView(ViewConfiguration viewConfiguration)
                => new ModsView(viewConfiguration);

            string IData.Name => "Mods";
        }

        sealed class ModDescriptionsType : DumpableObject, IData
        {
            Window IData.CreateView(ViewConfiguration viewConfiguration)
                => new ModDictionaryView(viewConfiguration);

            string IData.Name => "ModDescriptions";
        }

        internal ViewConfiguration(string [] identifier)
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

        internal Window View => ViewCache.Value;

        Window CreateAndConnectView()
        {
            var result = CreateView();
            ConnectToWindow(result);
            return result;
        }

        Window CreateView() {
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
            Status = "Closed";
            ViewCache.IsValid = false;
            MainContainer.Instance.RemoveViewConfiguration(this);
        }

        void OnActivated()
        {
            LastUsed = DateTime.Now;
            MainContainer.Instance.AddViewConfiguration(this);
        }

        internal string PositionPath => ItemFileName("Position");

        internal void ShowAndActivate()
        {
            View.Show();
            View.Activate();
        }

        internal bool IsMatching(string[] identifier)
        {
            NotImplementedFunction(this, identifier.Stringify(", "));
            return false;
        }
    }
}