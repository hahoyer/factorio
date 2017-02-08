using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    static class SystemConfiguration
    {
        const string ViewConfigurationFileName = "Views";
        const string ConfigurationRoot = "Mmasf";

        internal static string GetConfigurationPath(string name)
        {
            var fileHandle = ViewConfigurationPath
                .PathCombine(name.Replace("\\", "_"))
                .FileHandle();
            fileHandle.EnsureIsExistentDirectory();
            return fileHandle.FullName;
        }

        static string ViewConfigurationPath
            => ConfigurationRoot.PathCombine(ViewConfigurationFileName);
    }
}