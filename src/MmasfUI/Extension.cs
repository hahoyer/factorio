using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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

        public static UIElement CreateView(this MmasfContext instance)
        {
            var stackPanel = new StackPanel();

            foreach(var configuration in instance.UserConfigurations.Select(CreateView))
                stackPanel.Children.Add(configuration);

            return new ScrollViewer
            {
                Content = stackPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        static UIElement CreateView(this UserConfiguration configuration)
            => new TextBox
            {
                Text = configuration.Name
            };
    }
}