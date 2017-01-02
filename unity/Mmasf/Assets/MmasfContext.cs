using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using Common;

namespace Mmasf.Assets
{
    static class MmasfContext
    {
        public static readonly IManageModsAndSavefiles Instance = GetInstance();

        static IManageModsAndSavefiles GetInstance()
        {
            ChannelServices.RegisterChannel(new FileBasedClientChannel(), false);

            return
                (IManageModsAndSavefiles) Activator
                    .GetObject(typeof(IManageModsAndSavefiles), "filebased://localhost/Mmasf");
        }
    }
}