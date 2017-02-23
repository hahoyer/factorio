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

        readonly Proxy[] Data;
        DataGrid DataGrid;
        readonly StatusBar StatusBar = new StatusBar();

        public ModDictionaryView(ViewConfiguration viewConfiguration)
        {
            Data = MmasfContext
                .Instance
                .ModDictionary
                .Select(mods => CreateProxy(mods.Value))
                .ToArray();

            Content = CreateGrid();

            Title = viewConfiguration.Data.Name;
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
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
            DataGrid.ItemsSource = Data;
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
            var proxyItem = Data.Single(p => p.Data == item);
            DataGrid.SelectedItem = proxyItem;
        }
    }
}