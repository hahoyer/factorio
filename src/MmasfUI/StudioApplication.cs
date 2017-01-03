using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MmasfUI
{
    public sealed class StudioApplication : ApplicationContext, IStudioApplication
    {
        readonly List<Form> Children = new List<Form>();

        void IApplication.Register(Form child)
        {
            Children.Add(child);
            child.FormClosing += (a, s) => CheckedExit(child);
            child.Activated += (a, s) => OnActivated(child);
        }

        static void OnActivated(Form child) { throw new NotImplementedException(); }

        void IStudioApplication.Exit() => ExitThread();

        void CheckedExit(Form child)
        {
            Children.Remove(child);
            if(Children.Any(item => item.Visible))
                return;

            ExitThread();
        }

        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm

            var editorViews = SystemConfiguration
                .ActiveFileNames
                .Select(Open)
                .ToArray();

            foreach(var editorView in editorViews)
                editorView.Run();

            Application.Run(this);
        }

        View Open(FileConfiguration file) { throw new NotImplementedException(); }

        void IStudioApplication.Open() { throw new NotImplementedException(); }
    }

    interface IEditView
    {
        string FileName { get; }
    }
}