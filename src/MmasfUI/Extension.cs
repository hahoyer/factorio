using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        internal static MenuItem MenuItem(this string menuItemText, string commandIdentifier)
            => new MenuItem
            {
                Header = menuItemText,
                Command = MainContainer.Instance.CommandManager.ByName(commandIdentifier)
            };

        internal static ViewConfiguration SmartCreate
            (this ViewConfiguration.IData data, string name)
        {
            var modsConfiguration = new ViewConfiguration(name, data);
            if(modsConfiguration.Status == "Open")
                modsConfiguration.ShowAndActivate();
            return modsConfiguration;
        }

        public static void ConfigurateDefaultColumns(this DataGrid target)
        {
            TimeSpanProxy.Register(target);
            target.AutoGeneratingColumn += (s, e) => OnAutoGeneratingColumnsForConfigurateDefaultColumns(e);
        }

        public static void ActivateSelectedItems(this DataGrid target)
        {
            target.SelectionChanged += (s, e) => OnSelectionChangedForActivateSelectedItems(e);
        }

        static void OnAutoGeneratingColumnsForConfigurateDefaultColumns(DataGridAutoGeneratingColumnEventArgs args)
        {
            if(args.PropertyType == typeof(bool?))
                args.Column = new DataGridCheckBoxColumn
                {
                    Header = args.Column.Header,
                    Binding = new Binding(args.PropertyName),
                    IsThreeState = true
                };

            var dgbc = args.Column as DataGridBoundColumn;
            if(dgbc == null)
                return;

            var binding = (Binding) dgbc.Binding;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            if(args.PropertyType == typeof(DateTime))
            {
                binding.StringFormat = "u";
                dgbc.CanUserSort = true;
            }
        }

        static void OnSelectionChangedForActivateSelectedItems(SelectionChangedEventArgs args)
        {
            foreach(var item in args.RemovedItems)
                MainContainer.Instance.CommandManager.Activate(item, false);

            foreach(var item in args.AddedItems)
                MainContainer.Instance.CommandManager.Activate(item);
        }
    }
}