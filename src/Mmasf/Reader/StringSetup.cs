using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSavefiles.Reader
{
    sealed class StringSetup : Attribute
    {
        internal readonly Type CountType;
        public StringSetup(Type countType) { CountType = countType; }
    }
}