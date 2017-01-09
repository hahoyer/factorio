using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MmasfUI.Commands;

namespace MmasfUI
{
    sealed class ContextView : ChildView
    {
        public readonly IStudioApplication Parent;
        int CurrentConfigurationIndexValue;

        public ContextView(IStudioApplication parent)
            : base(parent)
        {
            Parent = parent;
            Client = parent.Context.CreateView();
            SetCurrentConfigurationIndex(0);
            Frame.Menu = this.CreateMenu();
            Title = "Factorio user configurations";
        }

        public int CurrentConfigurationIndex
        {
            get { return CurrentConfigurationIndexValue; }
            set
            {
                if(CurrentConfigurationIndexValue == value)
                    return;
                SetCurrentConfigurationIndex(value);
            }
        }

        void SetCurrentConfigurationIndex(int value)
        {
            var currentIndex = 0;
            foreach(var view in Client.GetUserConfigurationViews())
            {
                view.BackColor = currentIndex == value ? Color.Blue : Color.Gray;
                currentIndex++;
            }
            CurrentConfigurationIndexValue = value;
        }

        public void New() { throw new NotImplementedException(); }
    }


    public sealed class TransparentPanel : Panel
    {
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020;
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }
    }
}