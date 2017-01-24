using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    static class ViewExtension
    {
        internal static StackPanel CreateView
            (this UserConfiguration configuration, SolidColorBrush getIndicatorColor)
        {
            var data = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var header = new Label
            {
                Background = getIndicatorColor,
                MinHeight = 30,
                MinWidth = 20
            };

            var textBox = new Label
            {
                Content = configuration.Name
            };

            data.Children.Add(header);
            data.Children.Add(textBox);
            return data;
        }

        internal static SolidColorBrush GetIndicatorColor(this UserConfiguration configuration)
            => configuration.IsRoot
                ? (configuration.IsCurrent
                    ? Brushes.DarkBlue
                    : Brushes.LightBlue)
                : (configuration.IsCurrent
                    ? Brushes.Black
                    : Brushes.LightGray);


        static void SimulateSelections(ContextView view)
        {
            while(true)
                foreach(var configuration in MmasfContext.Instance.UserConfigurations)
                {
                    view.Dispatcher.Invoke(() => view.Selection.Current = configuration);
                    1.Seconds().Sleep();
                }
        }


        internal static ScrollViewer CreateContextView
            (this MmasfContext data, Selection<UserConfiguration> selection, ContextView parent)
        {
            var result = new ScrollViewer();
            var panel = new StackPanel();

            var elements = data
                .UserConfigurations
                .Select
                (
                    (configuration, index) =>
                        (UIElement) new UserConfigurationView
                        (
                            data,
                            configuration,
                            selection,
                            index,
                            parent
                        )
                );

            foreach(var configuration in elements)
                panel.Children.Add(configuration);

            result.Content = panel;
            result.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            return result;
        }

        internal static MenuItem MenuItem(this string menuItemText, string commandIdentifier)
            => new MenuItem
            {
                Header = menuItemText,
                Command = MainContainer.Instance.CommandManager.ByName(commandIdentifier)
            };
    }
}