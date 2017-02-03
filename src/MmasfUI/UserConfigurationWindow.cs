using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using hw.Helper;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Saves;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class UserConfigurationWindow : Window
    {
        readonly UserConfiguration Configuration;
        bool IsSaves;

        public UserConfigurationWindow(FileConfiguration fileConfiguration)
        {
            var configuration = MmasfContext
                .Instance
                .UserConfigurations.Single(u => u.Name == fileConfiguration.FileName);

            IsSaves = fileConfiguration.Type == FileConfiguration.SavesType;
            Configuration = configuration;
            Content = CreateGrid();
            Title = fileConfiguration.Type + " of " + configuration.Name.Quote();
            this.InstallPositionPersister(fileConfiguration.PositionPath);
            this.InstallMainMenu(CreateConfigurationMenu());
        }

        ScrollViewer CreateGrid()
        {
            var result = new DataGrid();

            result.Columns.Add
            (
                new DataGridTextColumn
                {
                    Binding = new Binding("Name"),
                    Header = "Name"
                }
            );
            result.Columns.Add
            (
                new DataGridTextColumn
                {
                    Binding = new Binding("Version"),
                    Header = "Version"
                }
            );

            result.ItemsSource = Configuration.SaveFiles.Select(s => new FileCluster(s)).ToArray();

            return new ScrollViewer
            {
                Content = result,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        sealed class FileCluster
        {
            public string Name;
            public Version Version;

            public FileCluster(ManageModsAndSavefiles.Saves.FileCluster fileCluster)
            {
                Name = fileCluster.Name;
                Version = fileCluster.Version;
            }
        }

        static Menu CreateConfigurationMenu()
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