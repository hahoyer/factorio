using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
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
        readonly FileClusterProxy[] Data;

        public UserConfigurationWindow(FileConfiguration fileConfiguration)
        {
            var configuration = MmasfContext
                .Instance
                .UserConfigurations.Single(u => u.Name == fileConfiguration.FileName);

            IsSaves = fileConfiguration.Type == FileConfiguration.SavesType;
            Configuration = configuration;

            Data = Configuration
                .SaveFiles
                .Take(10)
                .Select(s => new FileClusterProxy(s))
                .ToArray();

            Content = CreateGrid();

            Title = fileConfiguration.Type + " of " + configuration.Name.Quote();
            this.InstallPositionPersister(fileConfiguration.PositionPath);
            this.InstallMainMenu(CreateConfigurationMenu());
        }

        ScrollViewer CreateGrid()
        {
            var result = new DataGrid
            {
                ItemsSource = Data
            };

            Task.Factory.StartNew
            (
                () =>
                {
                    3000.MilliSeconds().Sleep();
                //    RefreshData();
                }
            );

            return new ScrollViewer
            {
                Content = result,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        void RefreshData()
        {
            if (!Dispatcher.CheckAccess()) 
            {
                Dispatcher.Invoke(RefreshData);
                return;
            }

            foreach (var proxy in Data)
                proxy.Refresh();
        }

        sealed class FileClusterProxy
        {
            readonly FileCluster FileCluster;

            public string Name => FileCluster.Name;

            public Version Version { get; set; }
            public TimeSpan Duration { get; set; }

            public FileClusterProxy(FileCluster fileCluster)
            {
                FileCluster = fileCluster;
                Version = fileCluster.Version;
                Duration = fileCluster.Duration;
            }

            public void Refresh()
            {
                Version = FileCluster.Version;
                Duration = FileCluster.Duration;
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