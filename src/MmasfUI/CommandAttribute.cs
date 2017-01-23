using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace MmasfUI
{
    [MeansImplicitUse, AttributeUsage(AttributeTargets.Method)]
    sealed class CommandAttribute : Attribute
    {
        internal readonly string Name;
        public CommandAttribute(string name) { Name = name; }
    }
}