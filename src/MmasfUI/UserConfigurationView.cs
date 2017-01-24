using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    public sealed class UserConfigurationView : ContentControl, Selection.IItemView
    {
        readonly MmasfContext Context;
        readonly UserConfiguration Configuration;

        internal UserConfigurationView
        (
            MmasfContext context,
            UserConfiguration configuration,
            Selection<UserConfiguration> selection,
            int index)
        {
            Context = context;
            Configuration = configuration;
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
        }

        bool Selection.IItemView.IsSelected
        {
            set { MainContainer.Instance.CommandManager.Activate(this, value); }
        }

        void Selection.IItemView.RegisterSelectionTrigger(Action value) { }
    }
}