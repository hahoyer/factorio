using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;

namespace MmasfUI
{
    public sealed class MainContainer : Window
    {
        readonly System.Windows.Controls.Label Label1;

        public MainContainer()
        {
            Width = 300;
            Height = 300;

            var grid = new Grid();
            Content = grid;

            var button1 = new System.Windows.Controls.Button
            {
                Content = "Say Hello!",
                Height = 23,
                Margin = new Thickness(96, 50, 107, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            button1.Click += button1_Click;
            grid.Children.Add(button1);

            Label1 = new System.Windows.Controls.Label();
            Label1.Margin = new Thickness(84, 115, 74, 119);
            grid.Children.Add(Label1);
        }

        void button1_Click(object sender, RoutedEventArgs e) { Label1.Content = "Hello WPF!"; }

        [STAThread]
        public static void Main()
        {
            var app = new System.Windows.Application();

            app.Run(new MainContainer());
        }
    }
}