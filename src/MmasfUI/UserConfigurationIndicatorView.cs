using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class UserConfigurationIndicatorView : Panel
    {
        public UserConfigurationIndicatorView(MmasfContext context, UserConfiguration configuration)
        {
            Size = new Size(10, 30);
            BackColor = context.GetIndicatorColor(configuration);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Tracer.FlaggedLine(e.Button + " at " +e.X + "/"  + e.Y);
            base.OnMouseClick(e);
        }
    }
}