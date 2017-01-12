using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MmasfUI
{
    public sealed class MainContainer : Window
    {
        readonly Label Label1;
        PositionConfig PositionConfig;

        string GetFileName() => Title.Select(ToValidFileChar).Aggregate("", (c, n) => c + n);

        public MainContainer()
        {
            PositionConfig = new PositionConfig(GetFileName)
            {
                Target = this
            };

            var grid = new Grid();
            Content = grid;

            var button1 = new Button
            {
                Content = "Say Hello!",
                Height = 23,
                Margin = new Thickness(96, 50, 107, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            button1.Click += button1_Click;
            grid.Children.Add(button1);

            Label1 = new Label();
            Label1.Margin = new Thickness(84, 115, 74, 119);
            grid.Children.Add(Label1);
        }

        static string ToValidFileChar(char c)
        {
            if (Path.GetInvalidFileNameChars().Contains(c))
                return "%" + (int)c;
            return "" + c;
        }

        void button1_Click(object sender, RoutedEventArgs e) { Label1.Content = "Hello WPF!"; }

        [STAThread]
        public static void Main()
        {
            var app = new Application();

            app.Run(new MainContainer());
        }
    }
}