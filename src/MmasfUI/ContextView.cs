using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    public sealed class ContextView : ContentControl
    {
        internal readonly Selection<UserConfiguration> Selection = new Selection<UserConfiguration>
            ();

        internal ContextView(MmasfContext data) { Content = data.CreateContextView(Selection); }

        [Command("UserConfigurations.New")]
        public void OnNew() { throw new NotImplementedException(); }

    }
}