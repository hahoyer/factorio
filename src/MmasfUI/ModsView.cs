using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Mods;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ModsView : Window, ViewConfiguration.IWindow
    {
        sealed class FileClusterProxy : INotifyPropertyChanged
        {
            readonly FileCluster Data;

            public FileClusterProxy(FileCluster data)
            {
                Data = data;
                OnPropertyChanged();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [UsedImplicitly]
            public bool IsEnabled => Data.IsEnabled == true;

            [UsedImplicitly]
            public string Name => Data.Name;

            [UsedImplicitly]
            public string Title => Data.Title;

            [UsedImplicitly]
            public Version Version => Data.Version;

            void OnPropertyChanged()
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        static Exception MultipleConfigurationException
            (IEnumerable<UserConfiguration> configurations) => throw new Exception
            ("MultipleConfiguration: " + configurations.Select(i => i.Name).Stringify(", "));


        static DataGrid CreateGrid(IEnumerable<FileClusterProxy> data)
        {
            var result = new DataGrid
            {
                IsReadOnly = true
            };

            result.AutoGeneratingColumn += (s, e) => OnAutoGeneratingColumns(e);
            result.ConfigurateDefaultColumns();
            result.ActivateSelectedItems();
            result.ItemsSource = data;

            return result;
        }

        static void OnAutoGeneratingColumns(DataGridAutoGeneratingColumnEventArgs args)
        {
            if(args.PropertyName != "Created")
                return;

            var column = args.Column as DataGridTextColumn;
            if(column == null)
                return;

            column.SortDirection = ListSortDirection.Descending;
            column.CanUserSort = true;
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

        readonly string ConfigurationName;
        readonly DataGrid DataGrid;
        readonly StatusBar StatusBar = new StatusBar();

        internal ModsView(ViewConfiguration viewConfiguration)
        {
            ConfigurationName = viewConfiguration.Identifier[1];
            DataGrid = CreateGrid(Data);
            Content = DataGrid;
            Title = viewConfiguration.Identifier.Stringify(" of ");
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        Window ViewConfiguration.IWindow.Window => this;
        void ViewConfiguration.IWindow.Refresh() => RefreshData();

        IEnumerable<FileClusterProxy> Data
        {
            get
            {
                return MmasfContext
                    .Instance
                    .UserConfigurations
                    .Top
                    (
                        u => u.Name == ConfigurationName,
                        multipleException: MultipleConfigurationException,
                        enableEmpty: false
                    )
                    .ModFiles
                    .Select(s => new FileClusterProxy(s))
                    .ToArray();
            }
        }

        void RefreshData()
        {
            var formerSelection = (FileClusterProxy) DataGrid.SelectedItem;
            DataGrid.ItemsSource = null;
            DataGrid.ItemsSource = Data;
            Select(formerSelection);
        }

        void Select(FileClusterProxy item)
        {
            var proxyItem = item == null ? null : Data.Single(p => p.Name == item.Name);
            DataGrid.SelectedItem = proxyItem;
        }
    }
}