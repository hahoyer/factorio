using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUI
{
    public interface IStudioApplication : IApplication
    {
        ManageModsAndSavefiles.MmasfContext Context { get; }
        void Exit();
    }
}