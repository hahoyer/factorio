using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    sealed class ContextView : ChildView
    {
        readonly MmasfContext Context;
        public ContextView(IStudioApplication parent)
            : base(parent)
        {
            Context = parent.Context;
            Client = Context.UserConfigurations.Select(CreateView).CreateRowView();
        }

        Control CreateView(UserConfiguration configuration)
        {
            var backColor = Color.LightGray;

            if(Context.DataConfiguration.RootUserConfigurationPath == configuration.Path)
                backColor = Color.Aqua;
            else if (Context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path)
                backColor = Color.Black;


            var indicator = new Panel
            {
                Size = new Size(10, 30),
                BackColor = backColor
            };

            return true.CreateLineupView(indicator, configuration.Name.CreateView());
        }
    }
}