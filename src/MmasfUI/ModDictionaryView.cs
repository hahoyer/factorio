using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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

            public Proxy(ModDescription data) { Data = data; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            void OnPropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string Name => Data.Name;
            public string Version => Data.Version.ToString();
        }

        readonly Proxy[] Data;
        DataGrid DataGrid;
        readonly StatusBar StatusBar = new StatusBar();

        public ModDictionaryView(ViewConfiguration viewConfiguration)
        {
            Data = MmasfContext
                .Instance
                .ModDictionary
                .SelectMany(mod => mod.Value.Select(version => new Proxy(version.Value)))
                .ToArray();

            Content = CreateGrid();

            Title = viewConfiguration.Name;
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        ScrollViewer CreateGrid()
        {
            DataGrid = new DataGrid
            {
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single
            };

            TimeSpanProxy.Register(DataGrid);

            DataGrid.ItemsSource = Data;

            return new ScrollViewer
            {
                Content = DataGrid,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
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