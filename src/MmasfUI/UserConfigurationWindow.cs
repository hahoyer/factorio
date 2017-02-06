using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
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
                    Tracer.FlaggedLine("waiting");
                    10.MilliSeconds().Sleep();

                    Tracer.FlaggedLine("Refreshing");
                    RefreshData();
                    Tracer.FlaggedLine("Refreshed");
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
            Tracer.FlaggedLine("loop");
            foreach(var proxy in Data)
                proxy.Refresh();
        }

        sealed class FileClusterProxy : INotifyPropertyChanged
        {
            readonly FileCluster FileCluster;
            Version VersionValue;
            TimeSpan DurationValue;

            public string Name => FileCluster.Name;

            public Version Version
            {
                get { return VersionValue; }
                set
                {
                    VersionValue = value;
                    OnPropertyChanged1();
                }
            }
            public TimeSpan Duration
            {
                get { return DurationValue; }
                set
                {
                    DurationValue = value;
                    OnPropertyChanged1();
                }
            }

            public FileClusterProxy(FileCluster fileCluster)
            {
                FileCluster = fileCluster;
                //Version = fileCluster.Version;
                //Duration = fileCluster.Duration;
            }

            public void Refresh()
            {
                Version = FileCluster.Version;
                Duration = FileCluster.Duration;
                1000.MilliSeconds().Sleep();
            }
            public event PropertyChangedEventHandler PropertyChanged;
            [NotifyPropertyChangedInvocator]
            void OnPropertyChanged1([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                Tracer.FlaggedLine("waiting");
                1000.MilliSeconds().Sleep();
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