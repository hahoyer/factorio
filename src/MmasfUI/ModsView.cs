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
    sealed class ModsView : Window
    {
        readonly StatusBar StatusBar = new StatusBar();

        internal ModsView(ViewConfiguration viewConfiguration)
        {
            var data = MmasfContext
                .Instance
                .UserConfigurations
                .Single(u => u.Name == viewConfiguration.Identifier[1])
                .ModFiles
                .Select(s => new FileClusterProxy(s))
                .ToArray();

            Content = CreateGrid(data);

            Title = viewConfiguration.Identifier.Stringify(" of ");
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

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
            if (args.PropertyName != "Created")
                return;

            var column = args.Column as DataGridTextColumn;
            if(column == null)
                return;

            column.SortDirection = ListSortDirection.Descending;
            column.CanUserSort = true;
        }

        sealed class FileClusterProxy : INotifyPropertyChanged
        {
            readonly FileCluster Data;

            [UsedImplicitly]
            public bool IsEnabled => Data.IsEnabled == true;
            [UsedImplicitly]
            public string Name => Data.Name;
            [UsedImplicitly]
            public string Title => Data.Title;
            [UsedImplicitly]
            public Version Version => Data.Version;

            public FileClusterProxy(FileCluster data)
            {
                Data = data;
                OnPropertyChanged();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged()
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
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