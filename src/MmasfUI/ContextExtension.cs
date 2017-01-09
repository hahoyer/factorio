using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    static class ContextExtension
    {
        internal static Color GetIndicatorColor
            (this MmasfContext context, UserConfiguration configuration)
            => context.DataConfiguration.RootUserConfigurationPath == configuration.Path
                ? (context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path
                    ? Color.DarkBlue
                    : Color.LightBlue)
                : (context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path
                    ? Color.Black
                    : Color.LightGray);

        internal static Control CreateView(this MmasfContext context)
        {
            var rows = context
                .UserConfigurations
                .Select(configuration => new UserConfigurationTile(context, configuration))
                .ToArray<Control>();

            var table = new Panel
            {
                Dock = DockStyle.Top
            };

            var height = 0;
            foreach(var row in rows)
            {
                row.Location = new Point(0, height);
                row.Width = table.Width;
                height += row.Height;
                row.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            }

            table.Height = height;
            table.Controls.AddRange(rows);

            var result = new Panel
            {
                AutoScroll = true
            };

            result.Controls.Add(table);
            return result;
        }

        internal static Control[] GetUserConfigurationViews(this Control control)
        {
            var results = control.Controls.Cast<Control>().ToArray();
            if(results.All(row => row is UserConfigurationTile))
                return results;

            return results.Single().GetUserConfigurationViews();
        }
    }
}