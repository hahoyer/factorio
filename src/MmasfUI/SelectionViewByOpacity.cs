using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;

namespace MmasfUI
{
    sealed class SelectionViewByOpacity : DumpableObject, Selection.IItemView
    {
        readonly UIElement Target;
        public SelectionViewByOpacity(UIElement target) { Target = target; }

        bool Selection.IItemView.Selection { set { Target.Opacity = value ? 0.2 : 0; } }
        Action Selection.IItemView.OnMouseClick { set { Target.MouseLeftButtonUp += (s, e) => value(); } }
    }
}