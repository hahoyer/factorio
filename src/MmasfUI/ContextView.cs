using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using ManageModsAndSavefiles;
using MmasfUI.Commands;

namespace MmasfUI
{
    sealed class ContextView : ChildView
    {
        public readonly IStudioApplication Parent;

        public ContextView(IStudioApplication parent)
            : base(parent)
        {
            Parent = parent;
            Client = parent.Context.CreateView();
            Frame.Menu = this.CreateMenu();
            Title = "Factorio user configurations";
        }

        public void New() { throw new NotImplementedException(); }
    }
}