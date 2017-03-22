using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using hw.Helper;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class SavesView : Window
    {
        readonly SaveFileClusterProxy[] Data;
        readonly StatusBar StatusBar = new StatusBar();

        public SavesView(ViewConfiguration viewConfiguration)
        {
            var configuration = MmasfContext
                .Instance
                .UserConfigurations.Single(u => u.Name == viewConfiguration.Name);

            var configurationName = configuration.Name;

            Data = configuration
                .SaveFiles
                .Select(s => new SaveFileClusterProxy(s, configurationName))
                .ToArray();

            ContextMenu = CreateContextMenu();

            Content = CreateGrid(Data);

            Task.Factory.StartNew
            (
                () =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    100.MilliSeconds().Sleep();
                    RefreshData();
                }
            );

            Title = viewConfiguration.Data.Name + " of " + configurationName.Quote();
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        static DataGrid CreateGrid(SaveFileClusterProxy[] data)
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

        static ContextMenu CreateContextMenu()
            => new ContextMenu
            {
                Items =
                {
                    "View _Conflicts".MenuItem(SaveFileClusterProxy.Command.ViewConflicts)
                }
            };

        static void OnAutoGeneratingColumns(DataGridAutoGeneratingColumnEventArgs args)
        {
            if (args.PropertyName != "Created")
                return;

            var column = args.Column as DataGridTextColumn;
            if (column == null)
                return;

            column.SortDirection = ListSortDirection.Descending;
            column.CanUserSort = true;
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