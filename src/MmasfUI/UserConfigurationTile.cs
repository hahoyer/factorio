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
            internal const string ViewMods = "UserConfiguration.ViewMods";
        }

        readonly MmasfContext Context;
        readonly UserConfiguration Configuration;
        new readonly ContextView Parent;
        readonly ViewConfiguration SavesConfiguration;
        readonly ViewConfiguration ModsConfiguration;

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

            SavesConfiguration = new ViewConfiguration(Configuration.Name, ViewConfiguration.TypeEnum.Saves);
            if(SavesConfiguration.Status == "Open")
                ViewSaves();

            ModsConfiguration = new ViewConfiguration(Configuration.Name, ViewConfiguration.TypeEnum.Mods);
            if (ModsConfiguration.Status == "Open")
                ViewMods();
        }

        static ContextMenu CreateContextMenu()
            => new ContextMenu
            {
                Items =
                {
                    "S_elect".MenuItem(Command.Select),
                    "Show _Saves".MenuItem(Command.ViewSaves),
                    "Show _Mods".MenuItem(Command.ViewMods)
                }
            };

        [Command(Command.ViewSaves)]
        public void ViewSaves()
        {
            var fileConfigurationView = SavesConfiguration.View;
            fileConfigurationView.Show();
            fileConfigurationView.Activate();
        }

        [Command(Command.ViewMods)]
        public void ViewMods()
        {
            var fileConfigurationView = ModsConfiguration.View;
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