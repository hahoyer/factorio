using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;

namespace MmasfUI
{
    partial class Selection
    {
        internal sealed class ViewByOpacity : DumpableObject, IAcceptor, IController
        {
            readonly UIElement Target;
            public ViewByOpacity(UIElement target) { Target = target; }

            bool IAcceptor.IsSelected { set { Target.Opacity = value ? 0.2 : 0; } }

            void IController.RegisterSelectionTrigger(Action value)
            {
                Target.MouseLeftButtonUp += (s, e) => value();
            }
        }

        internal sealed class List : DumpableObject, IAcceptor, IController
        {
            readonly IAcceptor[] Items;
            public List(params IAcceptor[] items) { Items = items; }

            bool IAcceptor.IsSelected
            {
                set
                {
                    foreach(var item in Items)
                        item.IsSelected = value;
                }
            }

            void IController.RegisterSelectionTrigger(Action value)
            {
                foreach(var item in Items.OfType<IController>())
                    item.RegisterSelectionTrigger(value);
            }
        }
    }
}