using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        public ModsView(ViewConfiguration viewConfiguration)
        {
            var configuration = MmasfContext
                .Instance
                .UserConfigurations.Single(u => u.Name == viewConfiguration.Name);

            var data = configuration
                .ModFiles
                .Select(s => new FileClusterProxy(s))
                .ToArray();

            Content = CreateGrid(data);

            Title = viewConfiguration.Name + " of " + configuration.Name.Quote();
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
            result.ItemsSource = data;

            return result;
        }

        static void OnAutoGeneratingColumns(DataGridAutoGeneratingColumnEventArgs args)
        {
            var column = args.Column as DataGridTextColumn;

            if(column == null)
                return;

            var binding = (Binding) column.Binding;
            if(args.PropertyType == typeof(DateTime))
            {
                binding.StringFormat = "u";
                column.CanUserSort = true;
            }

            if(args.PropertyName == "Created")
            {
                column.SortDirection = ListSortDirection.Descending;
                column.CanUserSort = true;
            }
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