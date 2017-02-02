using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        internal static readonly MainContainer Instance = new MainContainer();

        [STAThread]
        public static void Main() => Instance.Run();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShowContextView();

            var editorViews = SystemConfiguration
                .ActiveFileNames
                .Select(file => file.CreateView())
                .ToArray();

            foreach(var editorView in editorViews)
                editorView.Show();
        }

        void ShowContextView()
        {
            var view = MmasfContext.Instance.CreateView();
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
                            "_Select".MenuItem(UserConfigurationView.Command.Select),
                            "_Exit".MenuItem("Exit")
                        }
                    },
                    new MenuItem
                    {
                        Header = "_View",
                        Items =
                        {
                            "_Saves".MenuItem(UserConfigurationView.Command.ViewSaves)
                        }
                    }
                }
            };

        [Command("Exit")]
        public void OnExit() => Shutdown();

        internal readonly CommandManager CommandManager = new CommandManager
            (typeof(MainContainer).Namespace);
    }
}