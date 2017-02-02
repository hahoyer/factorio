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

        static System.Windows.Controls.Menu CreateConfigurationMenu()
            => new System.Windows.Controls.Menu
            {
                Items =
                {
                    new System.Windows.Controls.MenuItem
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