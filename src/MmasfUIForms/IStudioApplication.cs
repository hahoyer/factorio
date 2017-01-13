using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUIForms
{
    public interface IStudioApplication : IApplication
    {
        ManageModsAndSavefiles.MmasfContext Context { get; }
        void Exit();
    }
}