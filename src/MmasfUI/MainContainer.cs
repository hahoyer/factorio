using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Mods;
using MmasfUI.Common;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        [STAThread]
        public static void Main() => Instance.Run();

        internal static class Command
        {
            internal const string ViewModDictionary = "ViewModDictionary";
            internal const string RereadConfigurations = "RereadConfigurations";
        }

        internal static readonly MainContainer Instance = new MainContainer();

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
                            "_Select".MenuItem(UserConfigurationTile.Command.Select),
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
                            "Mod _dictionary".MenuItem(Command.ViewModDictionary)
                        }
                    },
                    new MenuItem
                    {
                        Header = "_Tools",
                        Items =
                        {
                            "_Reread Configurations".MenuItem(Command.RereadConfigurations)
                        }
                    }
                }
            };

        ViewConfiguration[] ViewConfigurations = new ViewConfiguration[0];

        protected override void OnStartup(StartupEventArgs e)
        {
            CleanupConfigArea();
            base.OnStartup(e);
            ShowContextView();
            SystemConfiguration.OpenActiveViews();
        }

        static void CleanupConfigArea() => SystemConfiguration.Cleanup();

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
            CommandManager.Activate(this);
            main.Show();
        }

        ViewConfiguration ModDictionary => GetViewConfiguration("ModDictionary");
        ModDictionaryView ModDictionaryView => (ModDictionaryView) ModDictionary.View;

        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary()
        {
            ModDictionaryView.RefreshData();
            ModDictionary.ShowAndActivate();
        }


        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary(ModDescription currentItem)
        {
            ModDictionaryView.RefreshData();
            ModDictionaryView.Select(currentItem);
            ModDictionary.ShowAndActivate();
        }

        [Command(Command.RereadConfigurations)]
        public void RereadConfigurations()
        {
            MmasfContext.Instance.RenewUserConfigurationPaths();
            ContextView.Refresh();
        }

        [Command("Exit")]
        public void OnExit()
        {
            CleanupConfigArea();
            IsClosing = true;
            Shutdown();
        }

        internal readonly CommandManager CommandManager
            = new CommandManager(typeof(MainContainer).Namespace);

        internal bool IsClosing;
        ContextView ContextView;

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