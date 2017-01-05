using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class UserConfigurationRowView : Panel
    {
        public UserConfigurationRowView(MmasfContext context, UserConfiguration configuration)
        {
            Margin = Padding.Empty;
            var control = context.CreateView(configuration);
            Tracer.FlaggedLine(configuration.Name + ": " + control.Size);
            control.Dock = DockStyle.Fill;
            Controls.Add(control);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Tracer.FlaggedLine(e.Button + " at " + e.X + "/" + e.Y);
            base.OnMouseClick(e);
        }
    }
}