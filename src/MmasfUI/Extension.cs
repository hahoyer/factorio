using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;

namespace MmasfUI
{
    static class Extension
    {
        public static string Dump(this Control value)
        {
            var thisDump =
                value.GetType().Name + DumpData(value).Surround("(", ")");

            if(value.Controls.Count == 0)
                return thisDump;

            var controlsDump = value
                .Controls
                .Cast<Control>()
                .Select(item => item.Dump())
                .Stringify("\n");
            return thisDump + controlsDump.Surround("(", ")");
        }

        static string DumpData(Control value) => new[]
            {
                "Location=" + value.Location,
                "Size=" + value.Size,
                "Dock=" + value.Dock,
                "Anchor=" + value.Anchor
            }
            .Stringify("\n");

        public static void DoLayout(this Control control)
        {
            var temp = new Panel();
            temp.Controls.Add(control);
            temp.Controls.Remove(control);
        }
    }
}