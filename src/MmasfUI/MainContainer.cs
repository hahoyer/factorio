using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using hw.DebugFormatter;
using ManageModsAndSavefiles.Mods;
using MmasfUI.Common;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        internal static class Command
        {
            internal const string ViewModDictionary = "ViewModDictionary";
        }


        internal static readonly MainContainer Instance = new MainContainer();

        [STAThread]
        public static void Main() => Instance.Run();

        ViewConfiguration ModDescriptionsValue;

        public ViewConfiguration ModDescriptions
            =>
                ModDescriptionsValue
                ?? (ModDescriptionsValue = ViewConfiguration.ModDescriptions.SmartCreate(""));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShowContextView();
        }

        void ShowContextView()
        {
            var view = new ContextView();
            var main = new Window
            {
                Content = view,
                Title = "MmasfContext"
            };

            view.Selection.RegisterKeyBoardHandler(main);
            main.InstallPositionPersister("Main");
            main.InstallMainMenu(CreateMainMenu());
            CommandManager.Activate(this);
            main.Show();
        }

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
                    }
                }
            };

        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary() => ModDescriptions.ShowAndActivate();

        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary(ModDescription currentItem)
        {
            ((ModDictionaryView)ModDescriptions.View).Select(currentItem);
            ModDescriptions.ShowAndActivate();
        }

        [Command("Exit")]
        public void OnExit() => Shutdown();

        internal readonly CommandManager CommandManager = new CommandManager
            (typeof(MainContainer).Namespace);
    }
}