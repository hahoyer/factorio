using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ManageModsAndSavefiles;

namespace Net2Assembly
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var client = IpcClient.Instance;
            //var info = client.GetObject<IContext>().Information;
            var info = MmasfContext.Instance.FactorioInformation;
            Debug.WriteLine(info);
        }
    }
}