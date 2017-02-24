using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Saves;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ModConflictsView : Window
    {
        sealed class Proxy : INotifyPropertyChanged
        {
            internal static class Command
            {
                internal const string ViewModDescriptions = "ModConflictsView.ViewModDescriptions";
            }

            readonly ModConflict Data;
            ModDescription Mod => Data.Mod;

            public Proxy(ModConflict data) { Data = data; }
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            void OnPropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string ModName => Mod.Name;
            public string SaveVersion => Data.SaveMod?.Version.ToString();
            public string GameVersion => Data.GameMod?.Version.ToString();

            [Command(Command.ViewModDescriptions)]
            public void ViewModDescriptions()
                => MainContainer
                    .Instance
                    .CommandManager
                    .ByName(MainContainer.Command.ViewModDictionary)
                    .Execute(Mod);
        }

        readonly Proxy[] Data;
        DataGrid DataGrid;
        readonly StatusBar StatusBar = new StatusBar();

        public ModConflictsView
            (ViewConfiguration viewConfiguration, ManageModsAndSavefiles.Saves.FileCluster parent)
        {
            Data = parent.RelevantConflicts
                .Select(s => new Proxy(s))
                .ToArray();

            ContextMenu = CreateContextMenu();
            Content = CreateGrid();

            Title = viewConfiguration.Data.Name
                + " of "
                + parent.Name.Quote()
                + " of "
                + parent.Parent.Name.Quote();
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        static ContextMenu CreateContextMenu()
            => new ContextMenu
            {
                Items =
                {
                    "View _Mod descriptions".MenuItem(Proxy.Command.ViewModDescriptions)
                }
            };

        DataGrid CreateGrid()
        {
            DataGrid = new DataGrid
            {
                IsReadOnly = true
            };

            DataGrid.SelectionChanged += (s, e) => OnSelectionChanged(e);

            TimeSpanProxy.Register(DataGrid);

            DataGrid.ItemsSource = Data;

            return DataGrid;
        }

        static void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            foreach(var item in args.RemovedItems)
                MainContainer.Instance.CommandManager.Activate(item, false);

            foreach(var item in args.AddedItems)
                MainContainer.Instance.CommandManager.Activate(item);
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
                    },
                    new MenuItem
                    {
                        Header = "_View",
                        Items =
                        {
                            "View _Mod descriptions".MenuItem(Proxy.Command.ViewModDescriptions)
                        }
                    }
                }
            };
    }
}