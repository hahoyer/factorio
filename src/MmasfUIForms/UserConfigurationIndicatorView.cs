using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUIForms
{
    sealed class UserConfigurationIndicatorView : Panel
    {
        public UserConfigurationIndicatorView(MmasfContext context, UserConfiguration configuration)
        {
            Size = new Size(10, 30);
            BackColor = context.GetIndicatorColor(configuration);
            Tracer.FlaggedLine(configuration.Name + ": " + Size);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Tracer.FlaggedLine(e.Button + " at " +e.X + "/"  + e.Y);
            base.OnMouseClick(e);
        }
    }
}