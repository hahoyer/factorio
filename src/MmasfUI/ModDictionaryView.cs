using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Mods;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ModDictionaryView : Window
    {
        sealed class Proxy : INotifyPropertyChanged
        {
            internal readonly ModDescription Data;

            public Proxy(ModDescription data, IEnumerable<Version> moreVersions)
            {
                Data = data;
                MoreVersions = moreVersions.Stringify(" ");
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            void OnPropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            [UsedImplicitly]
            public string Name => Data.Name;
            [UsedImplicitly]
            public string Version => Data.Version.ToString();
            [UsedImplicitly]
            public string MoreVersions { get; }
        }

        Proxy[] Data;
        DataGrid DataGrid;
        readonly StatusBar StatusBar = new StatusBar();

        public ModDictionaryView(ViewConfiguration viewConfiguration)
        {
            Content = CreateGrid();

            RefreshData();
            Title = viewConfiguration.Data.Name;
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        internal void RefreshData()
        {
            var formerSelection = ((Proxy) DataGrid.SelectedItem)?.Data;
            DataGrid.ItemsSource = null;
            Data = MmasfContext
                .Instance
                .ModDictionary
                .Where(mods => mods.Key != "base")
                .Select(mods => CreateProxy(mods.Value))
                .ToArray();
            DataGrid.ItemsSource = Data;
            Select(formerSelection);
        }

        static Proxy CreateProxy(FunctionCache<Version, ModDescription> modVersions)
        {
            var value = modVersions.OrderByDescending(mod => mod.Key).First().Value;
            var moreVersions = modVersions.Select(v => v.Key).Where(v => v != value.Version);
            return new Proxy(value, moreVersions);
        }

        DataGrid CreateGrid()
        {
            DataGrid = new DataGrid
            {
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single
            };

            TimeSpanProxy.Register(DataGrid);
            return DataGrid;
        }

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
    }
}