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

            var maxWidth = rows.Max(row => row.Width);
            foreach(var row in rows)
                row.Width = maxWidth;

            var table = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty
            };

            table.Controls.AddRange(rows);

            var result = new Panel
            {
                AutoScroll = true
            };
            result.Controls.Add(table);
            return result;
        }
    }
}