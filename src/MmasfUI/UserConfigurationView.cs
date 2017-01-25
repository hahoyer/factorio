using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ManageModsAndSavefiles;
using MmasfUI.Common;

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
            var data = Configuration.CreateView(Configuration.GetIndicatorColor());

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
                Configuration,
                Selection.List(this, Selection.ViewByOpacity(frame))
            );

            

            ContextMenu = CreateContextMenu();
            Content = result;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var command = MainContainer.Instance.CommandManager.ByName("UserConfiguration.OpenDetail");
                command.Execute(null);
            }
            base.OnKeyUp(e);
            
        }


        static ContextMenu CreateContextMenu()
            => new ContextMenu {Items = {"Select".MenuItem("UserConfiguration.Select")}};

        [Command("UserConfiguration.Select")]
        public void OnSelect()
        {
            Context.DataConfiguration.CurrentUserConfigurationPath = Configuration.Path;
            Parent.Refresh();
        }

        [Command("UserConfiguration.Select")]
        public bool CanExecuteSelect => !Configuration.IsCurrent;

        bool Selection.IAcceptor.IsSelected { set { MainContainer.Instance.CommandManager.Activate(this, value); } }
    }
}