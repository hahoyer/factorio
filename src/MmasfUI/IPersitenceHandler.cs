using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUI
{
    interface IPersitenceHandler<T>
    {
        T Get(string name);
        void Set(string name, T value);
    }
}