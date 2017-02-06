using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    static class Extension
    {
        internal static StackPanel CreateTileView
            (this UserConfiguration content, SolidColorBrush getIndicatorColor)
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
                Content = content.Name
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

        internal static ScrollViewer CreateView
            (this MmasfContext context, Selection<UserConfiguration> selection, ContextView parent)
        {
            var result = new ScrollViewer();
            var panel = new StackPanel();

            var elements = context
                .UserConfigurations
                .Select
                (
                    (configuration, index) =>
                        (UIElement) new UserConfigurationTile
                        (
                            context,
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

        internal static ContextView CreateView(this MmasfContext context)
            => new ContextView(context);

        internal static MenuItem MenuItem(this string menuItemText, string commandIdentifier)
            => new MenuItem
            {
                Header = menuItemText,
                Command = MainContainer.Instance.CommandManager.ByName(commandIdentifier)
            };
    }
}