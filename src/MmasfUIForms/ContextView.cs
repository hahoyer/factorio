using System;
using System.Collections.Generic;
using System.Linq;
using MmasfUIForms.Commands;

namespace MmasfUIForms
{
    sealed class ContextView : ChildView
    {
        public readonly IStudioApplication Parent;
        int CurrentConfigurationIndexValue;
        readonly UserConfigurationTile[] UserConfigurationTiles;

        public ContextView(IStudioApplication parent)
            : base(parent)
        {
            Parent = parent;
            Client = parent.Context.CreateView();
            UserConfigurationTiles = Client.GetUserConfigurationTiles();
            VisualizeCurrentConfigurationIndex(true);

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

                VisualizeCurrentConfigurationIndex(false);
                CurrentConfigurationIndexValue = value;
                VisualizeCurrentConfigurationIndex(true);
            }
        }

        void VisualizeCurrentConfigurationIndex(bool value)
        {
            UserConfigurationTiles[CurrentConfigurationIndex].Selection = value;
        }

        public void New() { throw new NotImplementedException(); }
    }
}