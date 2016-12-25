using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using hw.DebugFormatter;


namespace Client
{
    class MessageSink : DumpableObject, IMessageSink
    {
        readonly FileBasedClientChannel Parent;

        public MessageSink(FileBasedClientChannel parent) { Parent = parent; }

        IMessage IMessageSink.SyncProcessMessage(IMessage msg)
        {
            var m = msg as IMethodCallMessage;

            if(m != null)
            {
                var methodName = m.MethodName;
                var mm = (MethodInfo) m.MethodBase;
                var className = mm.DeclaringType.FullName;

                Tracer.Assert(!m.Args.Any());
                var result = Parent.Get(className, methodName, mm.ReturnType);
                return new ReturnMessage(result, null, 0, null, m);
            }
            NotImplementedMethod(msg.ToString());
            return null;
        }

        IMessageCtrl IMessageSink.AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            NotImplementedMethod(msg, replySink);
            return null;
        }

        IMessageSink IMessageSink.NextSink
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        object Serialize(object o)
        {
            NotImplementedMethod(o)
                ;
            return null;
        }
    }
}