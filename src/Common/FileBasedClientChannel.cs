using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using hw.DebugFormatter;

namespace Common
{
    public sealed class FileBasedClientChannel : DumpableObject, IChannelSender, ISecurableChannel
    {
        string IChannel.Parse(string urlString, out string objectURI)
        {
            objectURI = Parse(urlString);
            return objectURI;
        }

        int IChannel.ChannelPriority => 100;

        string IChannel.ChannelName => Constants.FileBasedSchemeName;

        IMessageSink IChannelSender.CreateMessageSink
            (string urlString, object remoteChannelData, out string objectURI)
        {
            objectURI = Parse(urlString);
            if(objectURI == null)
                return null;

            Tracer.Assert(remoteChannelData == null);
            return new MessageSink(objectURI);
        }

        bool ISecurableChannel.IsSecured { get; set; }

        static string Parse(string urlString)
        {
            var url = new Uri(urlString);
            Tracer.Assert(url.IsLoopback);
            Tracer.Assert(url.AbsolutePath.StartsWith("/"));
            var objectURI = url.AbsolutePath.Substring(1);
            return url.Scheme.ToLower() == Constants.FileBasedSchemeName ? objectURI : null;
        }
    }
}