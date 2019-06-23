using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ManageModsAndSaveFiles;
using ManageModsAndSaveFiles.Mods;
using MmasfUI.Common;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        internal static class Command
        {
            internal const string RereadConfigurations = "RereadConfigurations";
            internal const string RunFactorio = "RunFactorio";
            internal const string ViewModDictionary = "ViewModDictionary";
        }

        internal static readonly MainContainer Instance = new MainContainer();

        [STAThread]
        public static void Main() => Instance.Run();

        static Menu CreateMainMenu()
            => new Menu
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = "_File",
                        Items =
                        {
                            "_Open Location".MenuItem(UserConfigurationTile.Command.OpenLocation),
                            "_Select".MenuItem(UserConfigurationTile.Command.Select),
                            "Run _Lua".MenuItem(UserConfigurationTile.Command.RunLua),
                            "_Exit".MenuItem("Exit")
                        }
                    },
                    new MenuItem
                    {
                        Header = "_View",
                        Items =
                        {
                            "_Saves".MenuItem(UserConfigurationTile.Command.ViewSaves),
                            "_Mods".MenuItem(UserConfigurationTile.Command.ViewMods),
                            "Mod _dictionary".MenuItem(Command.ViewModDictionary),
                        }
                    },
                    new MenuItem
                    {
                        Header = "_Tools",
                        Items =
                        {
                            "Run _Factorio".MenuItem(Command.RunFactorio),
                            "_Reread Configurations".MenuItem(Command.RereadConfigurations)
                        }
                    }
                }
            };

        static void CleanupConfigArea() => SystemConfiguration.Cleanup();

        internal readonly CommandManager CommandManager
            = new CommandManager(typeof(MainContainer).Namespace);

        internal bool IsClosing;
        ContextView ContextView;

        ViewConfiguration[] ViewConfigurations = new ViewConfiguration[0];

        ViewConfiguration ModDictionary => GetViewConfiguration("ModDictionary");
        ModDictionaryView ModDictionaryView => (ModDictionaryView) ModDictionary.View;

        protected override void OnStartup(StartupEventArgs e)
        {
            CleanupConfigArea();
            base.OnStartup(e);
            ShowContextView();
            SystemConfiguration.OpenActiveViews();
        }

        void ShowContextView()
        {
            ContextView = new ContextView();
            var main = new Window
            {
                Content = ContextView,
                Title = "MmasfContext"
            };

            ContextView.Selection.RegisterKeyboardHandler(main);
            main.InstallPositionPersister("Main");
            main.InstallMainMenu(CreateMainMenu());
            CommandManager[this] = true;
            main.Show();
        }

        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary()
        {
            ModDictionaryView.RefreshData();
            ModDictionary.ShowAndActivate();
        }

        [Command(Command.RunFactorio)]
        public void RunFactorio() => Extension.RunFactorio();

        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary(ModDescription currentItem)
        {
            ModDictionaryView.RefreshData();
            ModDictionaryView.Select(currentItem);
            ModDictionary.ShowAndActivate();
        }

        [Command(Command.RereadConfigurations)]
        public void RereadConfigurations() {ContextView.RereadConfigurations();}

        [Command("Exit")]
        public void OnExit()
        {
            CleanupConfigArea();
            IsClosing = true;
            Shutdown();
        }

        internal void RemoveViewConfiguration(ViewConfiguration viewConfiguration)
        {
            ViewConfigurations =
                ViewConfigurations.Where(item => item != viewConfiguration).ToArray();
        }

        internal void AddViewConfiguration(ViewConfiguration viewConfiguration)
        {
            if(ViewConfigurations.All(item => item != viewConfiguration))
                ViewConfigurations = ViewConfigurations.Concat(new[] {viewConfiguration}).ToArray();
        }

        internal ViewConfiguration GetViewConfiguration(params string[] identifier)
            => FindViewConfiguration(identifier) ?? new ViewConfiguration(identifier);

        ViewConfiguration FindViewConfiguration(string[] identifier)
        {
            return ViewConfigurations.FirstOrDefault
                (item => item.Identifier.SequenceEqual(identifier));
        }

        public void RefreshAll()
        {
            foreach(var viewConfiguration in ViewConfigurations)
                viewConfiguration.Refresh();
        }
    }
}