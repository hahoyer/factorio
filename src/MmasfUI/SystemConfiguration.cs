using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace MmasfUI
{
    static class SystemConfiguration
    {
        const string ViewConfigurationFileName = "UI";

        internal static string ViewConfigurationPath
            => ManageModsAndSavefiles.SystemConfiguration
                .Folder
                .PathCombine(ViewConfigurationFileName);

        internal static string GetConfigurationPath(string name)
        {
            var fileHandle = StringExtender.ToSmbFile(ViewConfigurationPath
                .PathCombine(name.Replace("\\", "_")));
            fileHandle.EnsureIsExistentDirectory();
            return fileHandle.FullName;
        }
    }
}