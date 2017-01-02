using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class ManageModsAndSavefiles : MarshalByRefObject
    {
        public UserConfiguration[] UserConfigurations { get; set; }
        public string FactorioInformation { get; set; }
    }
}