using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    static class Constants
    {
        internal const string SystemWriteDataPlaceholder = "__PATH__system-write-data__";
        internal const string SystemReadDataPlaceholder = "__PATH__system-read-data__";

        internal static string SystemWriteData
            => Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .PathCombine("Factorio");

        internal static string SystemReadData
            => Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        internal const string ConfigFileName = "config.json";
        internal const string SystemConfigNameEnd = "\\Factorio\\config-path.cfg";
    }
}