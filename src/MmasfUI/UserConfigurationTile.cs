using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using ManageModsAndSaveFiles;
using MmasfUI.Common;

namespace MmasfUI
{
    public sealed class UserConfigurationTile : ContentControl, Selection.IAcceptor
    {
        internal static class Command
        {
            internal const string SelectAndRunFactorio = "UserConfiguration.SelectAndRunFactorio";
            internal const string OpenLocation = "UserConfiguration.OpenLocation";
            internal const string RunLua = "UserConfiguration.RunLua";
            internal const string Select = "UserConfiguration.Select";
            internal const string ViewMods = "UserConfiguration.ViewMods";
            internal const string ViewSaves = "UserConfiguration.ViewSaves";
        }

        static ContextMenu CreateContextMenu()
            => new ContextMenu
            {
                Items =
                {
                    "S_elect".MenuItem(Command.Select),
                    "Select and run _Factorio".MenuItem(Command.SelectAndRunFactorio),
                    "View _Saves".MenuItem(Command.ViewSaves),
                    "View _Mods".MenuItem(Command.ViewMods)
                }
            };

        readonly UserConfiguration Configuration;

        readonly MmasfContext Context;
        new readonly ContextView Parent;

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
        }


        bool Selection.IAcceptor.IsSelected {set => MainContainer.Instance.CommandManager[this] = value;}

        [Command(Command.Select)]
        [Command(Command.SelectAndRunFactorio)]
        public bool CanExecuteSelect => !Configuration.IsCurrent;

        [Command(Command.ViewSaves)]
        public void ViewSaves()
            => MainContainer
                .Instance
                .GetViewConfiguration("Saves", Configuration.Name)
                .ShowAndActivate();

        [Command(Command.ViewMods)]
        public void ViewMods()
            => MainContainer
                .Instance
                .GetViewConfiguration("Mods", Configuration.Name)
                .ShowAndActivate();

        [Command(Command.RunLua)]
        public void RunLua()
        {
            Configuration.RunLua();
            Parent.Refresh();
        }

        [Command(Command.Select)]
        public void OnSelect()
        {
            Context.DataConfiguration.CurrentUserConfigurationPath = Configuration.Path;
            Parent.Refresh();
        }

        [Command(Command.SelectAndRunFactorio)]
        public void OnSelectAndRunFactorio()
        {
            Context.DataConfiguration.CurrentUserConfigurationPath = Configuration.Path;
            Extension.RunFactorio();
            Parent.Refresh();
        }

        [Command(Command.OpenLocation)]
        public void OnOpenLocation()
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + "explorer " + Configuration.Path;
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}