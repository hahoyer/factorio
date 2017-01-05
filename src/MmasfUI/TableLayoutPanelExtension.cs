using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    static class TableLayoutPanelExtension
    {
        const int DefaultTextSize = 10;
        internal static readonly Control _dummy = new Control();

        internal static Control CreateGroup(this Control client, string title)
        {
            var result = new GroupBox
            {
                Text = title,
                AutoSize = true,
                Dock = DockStyle.Fill,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            client.Location = result.DisplayRectangle.Location;

            result.Controls.Add(client);
            return result;
        }

        internal static Control CreateColumnView(this IEnumerable<Control> controls)
            => InternalCreateLineupView(true, controls);

        internal static Control CreateRowView
        (
            this IEnumerable<Control> controls,
            TableLayoutPanelCellBorderStyle style = TableLayoutPanelCellBorderStyle.Single)
            => InternalCreateLineupView(false, controls, style);

        internal static Control CreateLineupView
            (this bool inColumns, params Control[] controls)
            => InternalCreateLineupView(inColumns, controls);

        internal static TableLayoutPanel ForceLineupView
            (this bool inColumns, params Control[] controls)
            => CreateTableLayoutPanel(inColumns, controls);

        internal static Control InternalCreateLineupView
        (
            bool useColumns,
            IEnumerable<Control> controls,
            TableLayoutPanelCellBorderStyle style = TableLayoutPanelCellBorderStyle.Single)
        {
            var effectiveControls = controls
                .Where(item => item != null && item != _dummy)
                .ToArray();
            return effectiveControls.Length == 1
                ? effectiveControls[0]
                : CreateTableLayoutPanel(useColumns, effectiveControls, style);
        }

        internal static TableLayoutPanel CreateTableLayoutPanel
        (
            bool inColumns,
            Control[] controls,
            TableLayoutPanelCellBorderStyle style = TableLayoutPanelCellBorderStyle.Single)
        {
            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = inColumns ? controls.Length : 1,
                RowCount = inColumns ? 1 : controls.Length,
                CellBorderStyle = style
            };

            result.Controls.AddRange(controls);
            return result;
        }

        internal static Control CreateView(this Dumpable dumpable)
            => CreateView(dumpable.Dump());

        internal static Label CreateView(this string text, double factor = 1, bool isBold = false)
            => new Label
            {
                Font = CreateFont(factor, isBold),
                AutoSize = true,
                Text = text
            };

        static Font CreateFont(double factor, bool isBold = false)
            =>
                new Font
                (
                    "Lucida Console",
                    (int) (DefaultTextSize * factor),
                    isBold ? FontStyle.Bold : FontStyle.Regular
                );

        internal static Label CreateView(this int value, double factor = 1, bool isBold = false)
            => new Label
            {
                Font = CreateFont(factor, isBold),
                AutoSize = true,
                Text = value.ToString(),
                TextAlign = ContentAlignment.MiddleRight
            };

        public interface IClickHandler
        {
            void Signal(object target);
        }

        internal static Control CreateLink(this object target, IClickHandler master)
        {
            var result = target.GetIdText().CreateView();
            result.Click += (s, a) => master.Signal(target);
            return result;
        }

        internal static string GetIdText(this object target)
        {
            var result = target.GetType().PrettyName();
            if(target is Dumpable)
                return result + "." + target.GetObjectId() + "i";
            return result;
        }

        internal static int GetObjectId(this object dumpable)
            => ((DumpableObject) dumpable).ObjectId;

        internal static int? GetObjectId<T>(this object dumpable)
        {
            if(dumpable is T)
                return ((DumpableObject) dumpable).ObjectId;

            return null;
        }
    }
}