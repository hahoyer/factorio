using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    public sealed class ContextView : ContentControl
    {
        public ContextView(MmasfContext data) { Content = CreateContextView(data); }

        static ScrollViewer CreateContextView(MmasfContext data)
        {
            var result = new ScrollViewer();
            var panel = new StackPanel();

            var selection = new Selection<UserConfiguration>();
            var elements = data
                .UserConfigurations
                .Select((configuration, index) => data.CreateView(configuration, selection, index));

            foreach(var configuration in elements)
                panel.Children.Add(configuration);

            result.Content = panel;
            result.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            return result;
        }
    }

    class Selection<T> {}
}