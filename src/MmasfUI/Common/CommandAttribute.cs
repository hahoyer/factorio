using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace MmasfUI.Common
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,AllowMultiple = true)]
    sealed class CommandAttribute : Attribute
    {
        internal readonly string Name;
        public CommandAttribute(string name) { Name = name; }
    }
}