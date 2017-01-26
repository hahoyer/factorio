using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class UserConfigurationSavesWindow : Window
    {
        readonly UserConfiguration Configuration;

        public UserConfigurationSavesWindow(UserConfiguration configuration)
        {
            Configuration = configuration;
            Content = CreateGrid();
            Title = "Configuration " + configuration.Name;
            this.InstallPositionPersister();
            this.InstallMainMenu(CreateConfigurationMenu());
        }

        Grid CreateGrid()
        {
            var result = new Grid();
            result.ColumnDefinitions.Add(IndexColumn);
            result.ColumnDefinitions.Add(NameColumn);
            foreach(var save in Configuration.SaveFiles)
                result.RowDefinitions.Add(new RowDefinition());

            return result;
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