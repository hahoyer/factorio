using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MmasfUI
{
    public interface IApplication
    {
        void Register(Form frame);
    }
}