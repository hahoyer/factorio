using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles;
using Newtonsoft.Json;

namespace MmasfUI
{
    static class Extension
    {
        internal static Rectangle ToRectangle(this Rect value)
            => new Rectangle
            {
                X = (int) value.X,
                Y = (int) value.Y,
                Height = (int) value.Height,
                Width = (int) value.Width
            };

        public static byte[] AsciiToByteArray(this string value) => Encoding.ASCII.GetBytes(value);

        public static void Sleep(this TimeSpan value) => Thread.Sleep(value);

        public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);
        public static void WriteLine(this string value) => Tracer.Line(value);

        public static T FromJson<T>(this string jsonText)
            => JsonConvert.DeserializeObject<T>(jsonText);

        public static object FromJson(this string jsonText, Type resultType)
            => JsonConvert.DeserializeObject(jsonText, resultType);

        public static string ToJson<T>(this T o)
            => JsonConvert.SerializeObject(o, Formatting.Indented);

        public static T FromJsonFile<T>(this string jsonFileName)
            where T : class
            => jsonFileName.FileHandle().String?.FromJson<T>();

        public static void ToJsonFile<T>(this string jsonFileName, T o)
            where T : class
            => jsonFileName.FileHandle().String = o.ToJson();

        public static string UnescapeComma(this string value)
            => value
                .Replace("&comma;", ",")
                .Replace("&ampersant;", "&");

        public static string EscapeComma(this string value)
            => value
                .Replace("&", "&ampersant;")
                .Replace(",", "&comma;");

        public static IEnumerable<T> EnsureAny<T>(this IEnumerable<T> value, Action onError)
        {
            var isAny = false;
            foreach(var item in value)
            {
                yield return item;

                isAny = true;
            }

            if(isAny)
                yield break;

            onError();
        }

        public static IEnumerable<T> EnsureNoDuplicate<T>(this IEnumerable<T> value, Action onError)
        {
            var isAny = false;
            foreach(var item in value)
            {
                if(isAny)
                {
                    onError();
                    yield break;
                }

                yield return item;

                isAny = true;
            }
        }

        internal static string ToValidFileChar(char c)
        {
            if(Path.GetInvalidFileNameChars().Contains(c))
                return "%" + (int) c;

            return "" + c;
        }

        internal static void InstallMainMenu(this Window container, Menu menu)
        {
            var content = (UIElement) container.Content;
            var d = new DockPanel();
            d.Children.Add(menu);
            DockPanel.SetDock(menu, Dock.Top);
            container.Content = d;
            if(content == null)
                return;

            d.Children.Add(content);
            DockPanel.SetDock(content, Dock.Bottom);
        }

        internal static void InstallPositionPersister(this Window main)
        {
            new PositionConfig
                (() => main.Title.Select(ToValidFileChar).Aggregate("", (c, n) => c + n))
                {
                    Target = main
                };
        }


        internal static UIElement CreateView(this MmasfContext instance)
            => new ContextView(instance);

        internal static UIElement CreateView
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
                Opacity= index==1?0.2:0,
                Background = System.Windows.Media.Brushes.Yellow
            };

            var result = new Grid();
            result.Children.Add(data);
            result.Children.Add(frame);
            return result;
        }

        static SolidColorBrush GetIndicatorColor
            (this MmasfContext context, UserConfiguration configuration)
            => context.DataConfiguration.RootUserConfigurationPath == configuration.Path
                ? (context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path
                    ? System.Windows.Media.Brushes.DarkBlue
                    : System.Windows.Media.Brushes.LightBlue)
                : (context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path
                    ? System.Windows.Media.Brushes.Black
                    : System.Windows.Media.Brushes.LightGray);

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
            var main = new Window
            {
                Content = MmasfContext.Instance.CreateView(),
                Title = "MmasfContext"
            };

            main.InstallPositionPersister();
            main.InstallMainMenu(application.CreateMainMenu());
            main.Show();

            var x = System.Windows.Markup.XamlWriter.Save(main);

        }
    }
}