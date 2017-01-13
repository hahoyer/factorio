using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MmasfUIForms
{
    public interface IApplication
    {
        void Register(Form frame);
    }
}