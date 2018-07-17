using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Mods;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ModDictionaryView : Window, ViewConfiguration.IWindow
    {
        internal static class Command
        {
            internal const string SaveConfiguration = "ModDescriptions.SaveConfiguration";
        }


        sealed class Proxy : INotifyPropertyChanged
        {
            internal readonly ModDescription Data;
            readonly Action OnPropertyEdited;

            public Proxy
                (ModDescription data, IEnumerable<Version> moreVersions, Action onPropertyEdited)
            {
                Data = data;
                OnPropertyEdited = onPropertyEdited;
                MoreVersions = moreVersions.Stringify(" ");
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            void OnPropertyChanged()
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

            [UsedImplicitly]
            public string Name => Data.Name;
            [UsedImplicitly]
            public string Version => Data.Version.ToString();

            [UsedImplicitly]
            public bool? SaveOnly
            {
                get { return Data.IsSaveOnlyPossible; }
                set
                {
                    Data.IsSaveOnlyPossible = value;
                    OnPropertyEdited();
                }
            }

            [UsedImplicitly]
            public bool? GameOnly
            {
                get { return Data.IsGameOnlyPossible; }
                set
                {
                    Data.IsGameOnlyPossible = value;
                    OnPropertyEdited();
                }
            }

            [UsedImplicitly]
            public string MoreVersions { get; }
        }

        Window ViewConfiguration.IWindow.Window => this;
        void ViewConfiguration.IWindow.Refresh() => RefreshData();

        Proxy[] Data;
        readonly DataGrid DataGrid;
        readonly StatusBar StatusBar = new StatusBar();
        readonly string RawTitle;

        public ModDictionaryView(ViewConfiguration viewConfiguration)
        {
            DataGrid = CreateGrid();

            Content = DataGrid;
            RefreshData();
            RawTitle = viewConfiguration.Identifier.Stringify(" / ");
            Title = RawTitle;
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
            MainContainer.Instance.CommandManager[this] = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            MainContainer.Instance.CommandManager[this] = false;
            base.OnClosed(e);
        }

        internal void RefreshData()
        {
            var formerSelection = ((Proxy) DataGrid.SelectedItem)?.Data;
            DataGrid.ItemsSource = null;
            Data = GetData();
            DataGrid.ItemsSource = Data;
            Select(formerSelection);
        }

        Proxy[] GetData()
            => MmasfContext
                .Instance
                .ModDictionary
                .Where(mods => mods.Key != "base")
                .Select(mods => CreateProxy(mods.Value))
                .ToArray();

        Proxy CreateProxy(FunctionCache<Version, ModDescription> modVersions)
        {
            var value = modVersions.OrderByDescending(mod => mod.Key).First().Value;
            var moreVersions = modVersions.Select(v => v.Key).Where(v => v != value.Version);
            return new Proxy(value, moreVersions, RefreshTitle);
        }

        static DataGrid CreateGrid()
        {
            var result = new DataGrid
            {
                SelectionMode = DataGridSelectionMode.Single
            };

            result.ConfigurateDefaultColumns();
            return result;
        }

        void RefreshTitle() { Title = RawTitle + (IsDirty ? "*" : ""); }

        static Menu CreateMenu()
            => new Menu
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = "_File",
                        Items =
                        {
                            "_Save".MenuItem(Command.SaveConfiguration),
                            "_Exit".MenuItem("Exit")
                        }
                    }
                }
            };

        internal void Select(ModDescription item)
        {
            var proxyItem = item == null ? null : Data.Single(p => p.Data.Name == item.Name);
            DataGrid.SelectedItem = proxyItem;
        }

        [DisableDump]
        [Command(Command.SaveConfiguration)]
        public bool IsDirty
            => MmasfContext
                .Instance
                .ModConfiguration
                .IsDirty;

        [Command(Command.SaveConfiguration)]
        public void SaveConfiguration()
        {
            MmasfContext
                .Instance
                .ModConfiguration
                .Save();
            RefreshTitle();
            MainContainer.Instance.RefreshAll();
        }
    }
}