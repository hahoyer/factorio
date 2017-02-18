using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles.Saves;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ModConflictsWindow : Window
    {
        sealed class Proxy : INotifyPropertyChanged
        {
            readonly ModConflict ModConflict;
            public Proxy(ModConflict modConflict) { ModConflict = modConflict; }
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            void OnPropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string Mod => (ModConflict.SaveMod ?? ModConflict.Mod).Name;
            public string SaveVersion => ModConflict.SaveMod?.Version.ToString();
            public string GameVersion => ModConflict.Mod?.Version.ToString();
        }

        readonly Proxy[] Data;
        DataGrid DataGrid;
        readonly StatusBar StatusBar = new StatusBar();

        public ModConflictsWindow(ViewConfiguration viewConfiguration, FileCluster parent)
        {
            var parent1 = parent;

            Data = parent1
                .Conflicts
                .Select(s => new Proxy(s))
                .ToArray();

            Content = CreateGrid();

            Title = viewConfiguration.Data.Name + " of " + parent1.Name.Quote();
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        ScrollViewer CreateGrid()
        {
            DataGrid = new DataGrid
            {
                IsReadOnly = true
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
    }
}