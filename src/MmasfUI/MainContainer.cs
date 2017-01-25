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

            var view = MmasfContext.Instance.CreateView();
            var main = new Window
            {
                Content = view,
                Title = "MmasfContext"
            };

            view.Selection.RegisterKeyBoardHandler(main);
            main.InstallPositionPersister();
            main.InstallMainMenu(CreateMainMenu());
            //Tracer.FlaggedLine("XAML: \n" + XDocument.Parse(XamlWriter.Save(main)));
            CommandManager.Activate(this);
            main.Show();


            //new Task(() => SimulateSelections(view)).Start();
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
                            "_Select".MenuItem("UserConfiguration.Select"),
                            "_Exit".MenuItem("Exit")
                        }
                    }
                }
            };

        [Command("Exit")]
        public void OnExit() => Shutdown();

        internal readonly CommandManager CommandManager = new CommandManager(typeof(MainContainer).Namespace);
    }
}