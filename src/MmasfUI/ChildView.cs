using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MmasfUI
{
    public abstract class ChildView : View
    {
        protected ChildView(IApplication master, string configFileName = null)
            : base(configFileName)
        {
            master.Register(Frame);
            Frame.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Frame.MaximizeBox = false;
            Frame.MinimizeBox = false;
        }

        internal void Run()
        {
            Frame.Show();
            Frame.BringToFront();
        }
    }
}