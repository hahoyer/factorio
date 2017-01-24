using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;

namespace MmasfUI
{
    partial class Selection
    {
        internal sealed class ViewByOpacity : DumpableObject, IItemView
        {
            readonly UIElement Target;
            public ViewByOpacity(UIElement target) { Target = target; }

            bool IItemView.IsSelected { set { Target.Opacity = value ? 0.2 : 0; } }

            void IItemView.RegisterSelectionTrigger(Action value)
            {
                Target.MouseLeftButtonUp += (s, e) => value();
            }
        }

        internal sealed class List : DumpableObject, IItemView
        {
            readonly IItemView[] Items;
            public List(params IItemView[] items) { Items = items; }

            bool IItemView.IsSelected
            {
                set
                {
                    foreach(var item in Items)
                        item.IsSelected = value;
                }
            }

            void IItemView.RegisterSelectionTrigger(Action value)
            {
                foreach(var item in Items)
                    item.RegisterSelectionTrigger(value);
            }
        }
    }
}