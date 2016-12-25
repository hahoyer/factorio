using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using Common;
using hw.DebugFormatter;
using hw.Helper;


namespace Client
{
    public class FileBasedClientChannel : DumpableObject, IChannelSender, ISecurableChannel
    {
        readonly string Directory;
        public FileBasedClientChannel(string directory) { Directory = directory; }

        string IChannel.Parse(string url, out string objectURI)
        {
            NotImplementedMethod(url, url);
            objectURI = null;
            return null;
        }

        int IChannel.ChannelPriority => 100;

        string IChannel.ChannelName => "MMasf";

        IMessageSink IChannelSender.CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            Tracer.Assert(url == "");
            Tracer.Assert(remoteChannelData == null);
            objectURI = "";
            return new MessageSink(this);
        }

        bool ISecurableChannel.IsSecured { get; set; }

        public object Get(string className, string methodName, Type resultType)
        {
            var c = new FileBasedCommunicatorClient(Directory.PathCombine(className));
            var result = c.Get(methodName).FromJson(resultType);

            throw new NotImplementedException();
        }
    }
}