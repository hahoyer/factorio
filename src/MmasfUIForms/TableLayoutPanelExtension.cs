using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MmasfUIForms
{
    static class TableLayoutPanelExtension
    {
        const int DefaultTextSize = 10;

        public static Font CreateFont(this double factor, bool isBold = false)
            =>
                new Font
                (
                    "Lucida Console",
                    (int) (DefaultTextSize * factor),
                    isBold ? FontStyle.Bold : FontStyle.Regular
                );

        public interface IClickHandler
        {
            void Signal(object target);
        }
    }
}