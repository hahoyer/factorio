using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using hw.Helper;

namespace MmasfUI
{
    class Commands : EnumEx
    {
        public static readonly ICommand New = new Command(ViewExtension.OnNew);
        public static readonly ICommand Select = new Command(ViewExtension.OnSelect);
        public static readonly ICommand Exit = new Command<MainContainer>(a => a.Shutdown());

        public static IEnumerable<ICommand> All => AllInstances<ICommand>();
    }
}