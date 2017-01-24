using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    public sealed class UserConfigurationView : ContentControl, Selection.IAcceptor
    {
        readonly MmasfContext Context;
        readonly UserConfiguration Configuration;
        new readonly ContextView Parent;

        internal UserConfigurationView
        (
            MmasfContext context,
            UserConfiguration configuration,
            Selection<UserConfiguration> selection,
            int index,
            ContextView parent)
        {
            Context = context;
            Configuration = configuration;
            Parent = parent;
            var data = configuration.CreateView(context.GetIndicatorColor(configuration));

            var frame = new Label
            {
                Opacity = 0,
                Background = Brushes.Yellow
            };

            var result = new Grid();
            result.Children.Add(data);
            result.Children.Add(frame);

            selection.Add
            (
                index,
                configuration,
                new Selection.List(this, new Selection.ViewByOpacity(frame)));
            Content = result;
        }

        [Command("UserConfiguration.Select")]
        public void OnSelect()
        {
            Context.DataConfiguration.CurrentUserConfigurationPath = Configuration.Path;
            Parent.Refresh();
        }

        bool Selection.IAcceptor.IsSelected
        {
            set { MainContainer.Instance.CommandManager.Activate(this, value); }
        }
    }
}