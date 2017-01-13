using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var main = new Window
            {
                Content = MmasfContext.Instance.CreateView(),
                Title = "MmasfContext"
            };

            main.InstallPositionPersister();
            main.InstallMainMenu(CreateMainMenu());
            main.Show();
        }

        Menu CreateMainMenu()
            => new Menu
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = "_File",
                        Items =
                        {
                            new MenuItem
                            {
                                Header = "_New",
                                Command = new Command(OnNew)
                            },
                            new MenuItem
                            {
                                Header = "_Exit",
                                Command = new Command(Shutdown)
                            }
                        }
                    }
                }
            };

        static void OnNew() { throw new NotImplementedException(); }

        [STAThread]
        public static void Main() => new MainContainer().Run();
    }
}