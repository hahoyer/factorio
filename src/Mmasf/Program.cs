using System;
using System.Collections.Generic;
using System.Linq;

namespace ManageModsAndSavefiles
{
    class Program
    {
        static void Main(string[] args)
        {
            UserConfiguration.Current.InitializeFrom(UserConfiguration.Original);
        }
    }
}