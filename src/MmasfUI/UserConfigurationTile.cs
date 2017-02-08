using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    public sealed class UserConfigurationTile : ContentControl, Selection.IAcceptor
    {
        internal static class Command
        {
            internal const string Select = "UserConfiguration.Select";
            internal const string ViewSaves = "UserConfiguration.ViewSaves";
        }

        readonly MmasfContext Context;
        readonly UserConfiguration Configuration;
        new readonly ContextView Parent;
        readonly ViewConfiguration ViewConfiguration;

        internal UserConfigurationTile
        (
            MmasfContext context,
            UserConfiguration configuration,
            Selection<UserConfiguration> selection,
            int index,
            ContextView parent)
        {
            Context = context;
            Configuration = configuration;
            Parent = parent;
            var data = Configuration.CreateTileView(Configuration.GetIndicatorColor());

            var frame = new Label
            {
                Opacity = 0,
                Background = Brushes.Yellow
            };

            var result = new Grid();
            result.Children.Add(data);
            result.Children.Add(frame);

            selection.Add
            (
                index,
                Configuration,
                Selection.List(this, Selection.ViewByOpacity(frame))
            );

            ContextMenu = CreateContextMenu();
            Content = result;

            ViewConfiguration = new ViewConfiguration(Configuration.Name, ViewConfiguration.SavesType);
            if(ViewConfiguration.Status == "Open")
                ViewSaves();
        }

        static ContextMenu CreateContextMenu()
            => new ContextMenu
            {
                Items =
                {
                    "Select".MenuItem(Command.Select),
                    "Show Saves".MenuItem(Command.ViewSaves)
                }
            };

        [Command(Command.ViewSaves)]
        public void ViewSaves()
        {
            var fileConfigurationView = ViewConfiguration.View;
            fileConfigurationView.Show();
            fileConfigurationView.Activate();
        }

        [Command(Command.Select)]
        public void OnSelect()
        {
            Context.DataConfiguration.CurrentUserConfigurationPath = Configuration.Path;
            Parent.Refresh();
        }

        [Command(Command.Select)]
        public bool CanExecuteSelect => !Configuration.IsCurrent;

        bool Selection.IAcceptor.IsSelected
        {
            set { MainContainer.Instance.CommandManager.Activate(this, value); }
        }
    }
}