using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using hw.DebugFormatter;
using hw.Helper;

namespace Common
{
    sealed class MessageSink : DumpableObject, IMessageSink
    {
        readonly string ObjectURI;

        public MessageSink(string objectURI) { ObjectURI = objectURI; }

        IMessage IMessageSink.SyncProcessMessage(IMessage msg)
        {
            var methodCallMessage = msg as IMethodCallMessage;
            if(methodCallMessage == null)
            {
                NotImplementedMethod(msg.ToString());
                return null;
            }

            Tracer.Assert(!methodCallMessage.HasVarArgs, "var-arguments are not supported.");

            Tracer.Assert
            (
                methodCallMessage.InArgCount == methodCallMessage.ArgCount,
                "Only in-arguments are supported"
            );

            var methodInfo = (MethodInfo) methodCallMessage.MethodBase;

            var result = SyncProcessMethod(methodInfo, methodCallMessage);
            return new ReturnMessage(result, null, 0, null, methodCallMessage);
        }

        object SyncProcessMethod(MethodInfo methodInfo, IMethodCallMessage methodCallMessage)
        {
            switch(methodInfo.MemberType)
            {
            case MemberTypes.Method:
                return Get(methodInfo, methodCallMessage.Args);
            case MemberTypes.Property:
                return SyncProcessProperty(methodCallMessage, methodInfo);
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        ReturnMessage SyncProcessProperty
            (IMethodCallMessage methodCallMessage, MethodInfo methodInfo)
        {
            var result = Get(methodInfo, methodCallMessage.Args);
            return new ReturnMessage(result, null, 0, null, methodCallMessage);
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

        object Get(MethodInfo method, object[] arguments)
        {
            var className = method.DeclaringType.AssertNotNull().FullName;
            var methodName = method.Name;
            var c = new FileBasedCommunicatorClient
                (Constants.RootPath.PathCombine(ObjectURI, className, methodName));
            var resultType = method.ReturnType;
            return c.Get(arguments.Select(a => a.ToJson()).ToArray()).FromJson(resultType);
        }
    }
}