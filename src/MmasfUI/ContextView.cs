using System;
using System.Collections.Generic;
using System.Linq;
using ManageModsAndSavefiles;
using MmasfUI.Commands;

namespace MmasfUI
{
    sealed class ContextView : ChildView
    {
        public readonly IStudioApplication Parent;
        readonly MmasfContext Context;

        public ContextView(IStudioApplication parent)
            : base(parent)
        {
            Parent = parent;
            Context = parent.Context;
            Client = Context.CreateView();

            Frame.Menu = this.CreateMenu();
            Title = "Factorio user configurations";
        }

        public void New() { throw new NotImplementedException(); }
    }
}