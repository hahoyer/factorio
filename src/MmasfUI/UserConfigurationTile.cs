using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class UserConfigurationTile : FlowLayoutPanel
    {
        public UserConfigurationTile(MmasfContext context, UserConfiguration configuration)
        {
            FlowDirection = FlowDirection.LeftToRight;
            AutoSize = true;
            WrapContents = false;
            BorderStyle = BorderStyle.Fixed3D;
            Border3DSide = 
            Margin = Padding.Empty;

            var indicator = new Panel
            {
                Size = new Size(10, 30),
                BackColor = context.GetIndicatorColor(configuration)
            };

            var name = new Label
            {
                Font = 1.0.CreateFont(),
                Text = configuration.Name
            };

            Controls.Add(indicator);
            Controls.Add(name);
            Tracer.FlaggedLine("\n" + GetType().Name + " " + configuration.Name + ": " + Size);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Tracer.FlaggedLine(e.Button + " at " + e.X + "/" + e.Y);
            base.OnMouseClick(e);
        }
    }
}