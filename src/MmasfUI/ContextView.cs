using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ManageModsAndSavefiles;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ContextView : ContentControl
    {

        internal readonly Selection<UserConfiguration> Selection
            = new Selection<UserConfiguration>();

        internal ContextView()
        {
            CreateView();
        }

        void CreateView() { Content = MmasfContext.Instance.CreateView(Selection, this); }

        internal void Refresh()
        {
            var oldSelection = Selection.Current;
            Selection.Current = null;
            CreateView();
            Selection.Current = oldSelection;
        }

    }
}