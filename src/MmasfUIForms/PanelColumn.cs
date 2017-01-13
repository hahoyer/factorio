using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MmasfUIForms
{
    sealed class PanelColumn : DataGridViewColumn
    {
        public override DataGridViewCell CellTemplate
        {
            get { return new PanelCell(); }
            set { base.CellTemplate = value; }
        }
    }
}