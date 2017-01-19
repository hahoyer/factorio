using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    static class ViewExtension
    {
        static UIElement CreateView
        (
            this MmasfContext context,
            UserConfiguration configuration,
            Selection<UserConfiguration> selection,
            int index)
        {
            var data = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var indicatorColor = GetIndicatorColor(context, configuration);
            var header = new Label
            {
                Background = indicatorColor,
                MinHeight = 30,
                MinWidth = 20
            };

            var textBox = new Label
            {
                Content = configuration.Name
            };

            data.Children.Add(header);
            data.Children.Add(textBox);

            var frame = new Label
            {
                Opacity = 0,
                Background = Brushes.Yellow
            };

            var result = new Grid();
            result.Children.Add(data);
            result.Children.Add(frame);

            selection.Add(configuration, index, new SelectionViewByOpacity(frame));
            return result;
        }

        static SolidColorBrush GetIndicatorColor
            (this MmasfContext context, UserConfiguration configuration)
            => context.DataConfiguration.RootUserConfigurationPath == configuration.Path
                ? (context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path
                    ? Brushes.DarkBlue
                    : Brushes.LightBlue)
                : (context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path
                    ? Brushes.Black
                    : Brushes.LightGray);

        static Menu CreateMainMenu(this Application application)
            => new Menu
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = "_File",
                        Items =
                        {
                            new MenuItem
                            {
                                Header = "_New",
                                Command = new Command(OnNew)
                            },
                            new MenuItem
                            {
                                Header = "_Exit",
                                Command = new Command(application.Shutdown)
                            }
                        }
                    }
                }
            };

        static void OnNew() { throw new NotImplementedException(); }

        internal static void CreateMainWindow(this Application application)
        {
            var view = MmasfContext.Instance.CreateView();
            var main = new Window
            {
                Content = view,
                Title = "MmasfContext"
            };

            main.InstallPositionPersister();
            main.InstallMainMenu(application.CreateMainMenu());
            main.Show();

            Tracer.FlaggedLine("XMAL: \n" + XDocument.Parse(XamlWriter.Save(main)));

            new Task(() => SimulateSelections(view)).Start();
        }

        static void SimulateSelections(ContextView view)
        {
            while(true)
            {
                foreach(var configuration in MmasfContext.Instance.UserConfigurations)
                {
                    view.Dispatcher.Invoke(() => view.Selection.Current = configuration);
                    1.Seconds().Sleep();
                }
            }
        }


        internal static ScrollViewer CreateContextView(this MmasfContext data, Selection<UserConfiguration> selection)
        {
            var result = new ScrollViewer();
            var panel = new StackPanel();

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
}