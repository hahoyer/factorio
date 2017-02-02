using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using hw.Helper;
using ManageModsAndSavefiles;
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
            var result = new Grid();
            result.ColumnDefinitions.Add(IndexColumn);
            result.ColumnDefinitions.Add(NameColumn);

            var index = 0;
            foreach(var save in Configuration.SaveFiles)
            {
                result.RowDefinitions.Add(new RowDefinition());

                var indexCell = new TextBlock
                {
                    Text = index.ToString()
                };

                Grid.SetColumn(indexCell, 0);
                Grid.SetRow(indexCell, index);

                var nameCell = new TextBlock
                {
                    Text = save.Name
                };

                Grid.SetColumn(nameCell, 1);
                Grid.SetRow(nameCell, index);

                result.Children.Add(indexCell);
                result.Children.Add(nameCell);

                index++;
            }

            return new ScrollViewer
            {
                Content = result,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        static ColumnDefinition IndexColumn => new ColumnDefinition();
        static ColumnDefinition NameColumn => new ColumnDefinition();

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