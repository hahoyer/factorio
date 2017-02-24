using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using hw.Helper;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class SavesView : Window
    {
        readonly SaveFileClusterProxy[] Data;
        readonly StatusBar StatusBar = new StatusBar();
        DataGrid DataGrid;

        public SavesView(ViewConfiguration viewConfiguration)
        {
            var configuration = MmasfContext
                .Instance
                .UserConfigurations.Single(u => u.Name == viewConfiguration.Name);

            Data = configuration
                .SaveFiles
                .Select(s => new SaveFileClusterProxy(s, configuration.Name))
                .ToArray();

            ContextMenu = CreateContextMenu();
            Content = CreateGrid();

            Title = viewConfiguration.Data.Name + " of " + configuration.Name.Quote();
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        DataGrid CreateGrid()
        {
            DataGrid = new DataGrid
            {
                IsReadOnly = true
            };

            DataGrid.AutoGeneratingColumn += (s, e) => OnAutoGeneratingColumns(e);
            DataGrid.SelectionChanged += (s, e) => OnSelectionChanged(e);

            TimeSpanProxy.Register(DataGrid);

            DataGrid.ItemsSource = Data;

            Task.Factory.StartNew
            (
                () =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    100.MilliSeconds().Sleep();
                    RefreshData();
                }
            );

            return DataGrid;
        }

        static ContextMenu CreateContextMenu()
            => new ContextMenu
            {
                Items =
                {
                    "View _Conflicts".MenuItem(SaveFileClusterProxy.Command.ViewConflicts),
                }
            };

        static void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            foreach(var item in args.RemovedItems)
                MainContainer.Instance.CommandManager.Activate(item, false);

            foreach(var item in args.AddedItems)
                MainContainer.Instance.CommandManager.Activate(item);
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

        void RefreshData()
        {
            var count = Data.Length;
            var current = 0;
            Parallel.ForEach
            (
                Data,
                proxy =>
                {
                    proxy.Refresh();
                    current++;
                    StatusBar.Text = current + " of " + count;
                });

            StatusBar.Text = count.ToString();
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
                            "View _Conflicts".MenuItem(SaveFileClusterProxy.Command.ViewConflicts)
                        }
                    }
                }
            };
    }
}