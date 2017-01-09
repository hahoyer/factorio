using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using hw.Helper;
using MmasfUI.Commands;

namespace MmasfUI
{
    sealed class ContextView : ChildView
    {
        public readonly IStudioApplication Parent;
        int CurrentConfigurationIndexValue;
        readonly ValueCache<UserConfigurationTile[]> UserConfigurationTilesCache;

        public ContextView(IStudioApplication parent)
            : base(parent)
        {
            Parent = parent;
            Client = parent.Context.CreateView();
            UserConfigurationTilesCache = new ValueCache<UserConfigurationTile[]>(Client.GetUserConfigurationTiles);
            SetCurrentConfigurationIndex(true);
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

                SetCurrentConfigurationIndex(false);
                CurrentConfigurationIndexValue = value;
                SetCurrentConfigurationIndex(true);
            }
        }

        void SetCurrentConfigurationIndex(bool value)
        {
            UserConfigurationTilesCache.Value[CurrentConfigurationIndex].Selection = value;
        }

        public void New() { throw new NotImplementedException(); }
    }
}