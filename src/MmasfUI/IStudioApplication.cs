using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUI
{
    public interface IStudioApplication : IApplication
    {
        void Exit();
        void Open();
    }
}