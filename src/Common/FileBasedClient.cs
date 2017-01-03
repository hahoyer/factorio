using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using hw.DebugFormatter;

namespace Common
{
    public sealed class FileBasedClient : DumpableObject
    {
        readonly string Name;

        public FileBasedClient(string name)
        {
            ChannelServices.RegisterChannel(new FileBasedClientChannel(), false);
            Name = name;
        }

        public T Get<T>()
            => (T) Activator.GetObject(typeof(T), "filebased://localhost/" + Name, this);
    }
}                                     