using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.CreateMainWindow();
        }

        [STAThread]
        public static void Main() => new MainContainer().Run();
    }
}