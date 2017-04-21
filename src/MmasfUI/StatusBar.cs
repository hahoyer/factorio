using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class StatusBar : System.Windows.Controls.Primitives.StatusBar
    {
        readonly TextBlock TextBlock;
        string TextValue;
        bool IsLocked;
        bool IsDirty;

        public StatusBar()
        {
            TextBlock = new TextBlock();
            Items.Add(TextBlock);
        }

        internal string Text
        {
            get { return TextValue; }
            set
            {
                if (TextValue == value)
                    return;

                lock (this)
                {
                    TextValue = value;
                    IsDirty = true;

                    PerformModification();
                }
            }
        }

        void PerformModification()
        {
            if(IsLocked)
                return;

            Task.Factory.StartNew(LateRefresh);
        }

        void Refresh()
        {
            if(!IsDirty)
                return;

            this.Synchronized
                (
                    () =>
                    {
                        lock(this)
                        {
                            TextBlock.Text = TextValue;
                            IsDirty = false;
                        }
                    }
                );
        }

        void LateRefresh()
        {
            IsLocked = true;

            do
            {
                Refresh();
                100.MilliSeconds().Sleep();
            } while(IsDirty);

            IsLocked = false;
        }
    }
}