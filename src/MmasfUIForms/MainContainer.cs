using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUIForms
{
    static class MainContainer
    {
        [STAThread]
        public static void Main()
        {
            //Tracer.IsBreakDisabled = true;
            new StudioApplication().Run();
        }
    }
}