using System.Windows.Controls;
using ManageModsAndSaveFiles;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class ContextView : ContentControl
    {
        internal readonly Selection<UserConfiguration> Selection
            = new Selection<UserConfiguration>();

        internal ContextView()
        {
            MmasfContext.Instance.OnExternalModification = () => Dispatcher.Invoke(Refresh);
            MmasfContext.Instance.OnModificationOnConfigPaths = () => Dispatcher.Invoke(RereadConfigurations);
            CreateView();
        }

        void CreateView()
        {
            Content = MmasfContext.Instance.CreateView(Selection, this);
            MmasfContext.Instance.ActivateWatcher();
        }

        internal void Refresh()
        {
            var oldSelection = Selection.Current;
            Selection.Current = null;
            CreateView();
            Selection.Current = oldSelection;
        }

        public void RereadConfigurations()
        {
            MmasfContext.Instance.RenewUserConfigurationPaths();
            Refresh();
        }
    }
}