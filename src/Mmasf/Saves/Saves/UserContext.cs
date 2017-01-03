using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using ManageModsAndSavefiles.Reader;

namespace ManageModsAndSavefiles.Saves
{
    sealed class UserContext : DumpableObject, BinaryRead.IContext
    {
        Version Version;
        public bool IsBefore013 => Version < new Version(0, 13);
        public bool Is01392 => Version == new Version(0, 13, 9, 2);
        public bool IsBefore01491 => Version < new Version(0, 14, 9, 1);
        public bool IsBefore01414 => Version < new Version(0, 14, 14);


        void BinaryRead.IContext.Got
            (BinaryRead reader, MemberInfo member, object captureIdentifier, object result)
        {
            if(captureIdentifier as string == "Version")
            {
                Version = (Version) result;
                return;
            }

            if(captureIdentifier as string == "Lookahead")
            {
                var value = reader.GetBytes(100);
                Tracer.TraceBreak();
                return;
            }

            NotImplementedMethod(member.Name, captureIdentifier, result.ToString());
        }
    }
}