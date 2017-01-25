using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;

namespace MmasfUI.Common
{
    partial class Selection
    {
        internal static IAcceptor ViewByOpacity(UIElement target) => new ViewByOpacityClass(target);
        internal static IAcceptor List(params IAcceptor[] items) => new ListClass(items);

        sealed class ViewByOpacityClass : DumpableObject, IAcceptor, IController
        {
            readonly UIElement Target;
            public ViewByOpacityClass(UIElement target) { Target = target; }

            bool IAcceptor.IsSelected { set { Target.Opacity = value ? 0.2 : 0; } }

            void IController.RegisterSelectionTrigger(Action value)
            {
                Target.MouseLeftButtonDown += (s, e) => value();
                Target.MouseRightButtonDown += (s, e) => value();
            }
        }

        sealed class ListClass : DumpableObject, IAcceptor, IController
        {
            readonly IAcceptor[] Items;
            public ListClass(IAcceptor[] items) { Items = items; }

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