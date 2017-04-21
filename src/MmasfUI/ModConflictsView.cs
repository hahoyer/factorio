using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Saves;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ModConflictsView : DumpableObject, ViewConfiguration.IWindow
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

            [UsedImplicitly]
            [NotifyPropertyChangedInvocator]
            void OnPropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            [UsedImplicitly]
            public string ModName => Mod.Name;
            [UsedImplicitly]
            public string SaveVersion => Data.SaveMod?.Version.ToString();
            [UsedImplicitly]
            public string GameVersion => Data.GameMod?.Version.ToString();
            [UsedImplicitly]
            public bool IsKnown => Data.IsKnown;

            [Command(Command.ViewModDescriptions)]
            public void ViewModDescriptions()
                => MainContainer
                    .Instance
                    .CommandManager
                    .ByName(MainContainer.Command.ViewModDictionary)
                    .Execute(Mod);
        }

        readonly Window Window;
        readonly string SaveFileName;
        readonly string ConfigurationName;
        readonly DataGrid DataGrid;

        internal ModConflictsView(ViewConfiguration viewConfiguration)
        {
            SaveFileName = viewConfiguration.Identifier[1];
            ConfigurationName = viewConfiguration.Identifier[2];
            DataGrid = CreateGrid(Data);
            Window = CreateWindow(viewConfiguration, DataGrid);
        }

        Window ViewConfiguration.IWindow.Window => Window;
        void ViewConfiguration.IWindow.Refresh() => DataGrid.ItemsSource = Data;

        static Window CreateWindow(ViewConfiguration viewConfiguration, DataGrid grid)
        {
            var window = new Window
            {
                ContextMenu = CreateContextMenu(),
                Content = grid,
                Title = viewConfiguration.Identifier.Stringify(" of ")
            };

            window.InstallPositionPersister(viewConfiguration.PositionPath);
            window.InstallMainMenu(CreateMenu());
            return window;
        }

        Proxy[] Data
            => MmasfContext
                .Instance
                .UserConfigurations
                .Single(u => u.Name == ConfigurationName)
                .SaveFiles
                .Single(u => u.Name == SaveFileName)
                .RelevantConflicts
                .Select(s => new Proxy(s))
                .ToArray();

        static ContextMenu CreateContextMenu()
            => new ContextMenu
            {
                Items =
                {
                    "View _Mod descriptions".MenuItem(Proxy.Command.ViewModDescriptions)
                }
            };

        static DataGrid CreateGrid(IEnumerable<Proxy> data)
        {
            var result = new DataGrid
            {
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single
            };

            result.ConfigurateDefaultColumns();
            result.ActivateSelectedItems();
            result.ItemsSource = data;
            return result;
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