using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class UserConfigurationTile : Panel
    {
        readonly Color SelectedColor;
        readonly Color NotSelectedColor;

        public UserConfigurationTile(MmasfContext context, UserConfiguration configuration)
        {
            BorderStyle = BorderStyle.FixedSingle;

            var indicator = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(10, 30),
                BackColor = context.GetIndicatorColor(configuration)
            };

            var name = new Label
            {
                Location = new Point(indicator.Width, 0),
                Font = 1.0.CreateFont(),
                Text = configuration.Name,
                AutoSize = true
            };

            NotSelectedColor = BackColor;
            SelectedColor = Color.LightGoldenrodYellow;

            name.DoLayout();

            Size = new Size(indicator.Width + name.Width, Math.Max(indicator.Height, name.Height));

            Controls.Add(indicator);
            Controls.Add(name);
        }

        public bool Selection
        {
            get { return SelectedColor == BackColor; }
            set { BackColor = value ? SelectedColor : NotSelectedColor; }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Tracer.FlaggedLine(e.Button + " at " + e.X + "/" + e.Y);
            base.OnMouseClick(e);
        }
    }
}