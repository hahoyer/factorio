using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;

namespace MmasfUI
{
    public abstract class View : DumpableObject
    {
        protected readonly Form Frame;
        PositionConfig PositionConfig;

        protected View(string configFileName = null)
        {
            Frame = new Form
            {
                Name = "Frame",
                Text = "?"
            };

            Frame.Closing += OnClosing;

            if(configFileName == null)
                configFileName = GetFileName();
            configFileName.FileHandle().EnsureDirectoryOfFileExists();

            PositionConfig = new PositionConfig(()=> configFileName)
            {
                Target = Frame
            };
        }

        internal string Title { get { return Frame.Text; } set { Frame.Text = value; } }

        string GetFileName() => Frame.Text.Select(ToValidFileChar).Aggregate("", (c, n) => c + n);

        static string ToValidFileChar(char c)
        {
            if(Path.GetInvalidFileNameChars().Contains(c))
                return "%" + (int) c;
            return "" + c;
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Frame.Visible = false;
        }

        internal Control Client
        {
            get { return Frame.Controls.Cast<Control>().FirstOrDefault(); }
            set
            {
                Tracer.Assert(Frame.Controls.Count == 0);
                value.Dock = DockStyle.Fill;
                Frame.Controls.Add(value);
            }
        }
    }
}