using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class ContextView : ContentControl
    {
        readonly MmasfContext Context;
        internal readonly Selection<UserConfiguration> Selection
            = new Selection<UserConfiguration>();

        internal ContextView(MmasfContext context)
        {
            Context = context;
            CreateView();
        }

        void CreateView() { Content = Context.CreateContextView(Selection, this); }

        [Command("UserConfigurations.New")]
        public void OnNew() { throw new NotImplementedException(); }

        internal void Refresh()
        {
            var oldSelection = Selection.Current;
            Selection.Current = null;
            CreateView();
            Selection.Current = oldSelection;
        }
    }
}