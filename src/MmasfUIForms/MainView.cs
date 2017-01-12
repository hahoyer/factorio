using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MmasfUI
{
    public abstract class MainView : View, IApplication
    {
        readonly ApplicationContext Context;

        protected MainView(string name, string configFileName = null)
            : base(configFileName)
        {
            Context = new ApplicationContext(Frame);
            Title = name;
            Frame.FormClosing += (a, b) => Application.Exit();
        }

        void IApplication.Register(Form child)
        {
            Frame.Closing += (a, s) => child.Close();
        }

        internal void Run() => Application.Run(Context);
    }
}