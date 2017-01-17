using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class ContextView : ScrollViewer
    {
        public ContextView(MmasfContext data)
        {
            var panel = new StackPanel();

            foreach(var configuration in data.UserConfigurations.Select(data.CreateView))
                panel.Children.Add(configuration);

            Content = panel;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
    }
}