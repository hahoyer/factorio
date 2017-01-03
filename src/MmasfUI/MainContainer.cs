using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUI
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