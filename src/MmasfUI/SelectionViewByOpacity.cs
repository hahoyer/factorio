using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;

namespace MmasfUI
{
    sealed class SelectionViewByOpacity : DumpableObject, ISelectionItemView
    {
        readonly UIElement Target;
        public SelectionViewByOpacity(UIElement target) { Target = target; }

        bool ISelectionItemView.Selection { set { Target.Opacity = value ? 0.2 : 0; } }
    }
}