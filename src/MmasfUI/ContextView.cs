using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class ContextView : ContentControl
    {
        public ContextView(MmasfContext data)
        {
            var view = CreateContextView(data);
            Content = (view);
            DockPanel.SetDock(view,Dock.Bottom| Dock.Left| Dock.Right| Dock.Top);
        }

        static ScrollViewer CreateContextView(MmasfContext data)
        {
            var result = new ScrollViewer();
            var panel = new StackPanel();

            foreach(var configuration in data.UserConfigurations.Select(data.CreateView))
                panel.Children.Add(configuration);

            result.Content = panel;
            result.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            return result;
        }
    }
}