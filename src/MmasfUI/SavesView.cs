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
    sealed class SavesView : Window, ViewConfiguration.IWindow
    {
        readonly SaveFileClusterProxy[] Data;
        readonly StatusBar StatusBar = new StatusBar();

        Window ViewConfiguration.IWindow.Window => this;
        void ViewConfiguration.IWindow.Refresh() => RefreshData();

        internal SavesView(ViewConfiguration viewConfiguration)
        {
            var configurationName = viewConfiguration.Identifier[1];

            Data = MmasfContext
                .Instance
                .UserConfigurations
                .Single(u => u.Name == configurationName)
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

            Title = viewConfiguration.Identifier.Stringify(" of ");
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
            if(args.PropertyName != "Created")
                return;

            var column = args.Column as DataGridTextColumn;
            if(column == null)
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