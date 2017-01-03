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
        readonly FileBasedClient Parent;

        public MessageSink(string objectURI, FileBasedClient parent)
        {
            ObjectURI = objectURI;
            Parent = parent;
        }

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
            if(methodInfo.IsSpecialName
                && methodInfo.Name.StartsWith("get_")
                && !methodInfo.GetParameters().Any())
                return Get(methodInfo);

            var property = methodInfo
                .DeclaringType
                .AssertNotNull()
                .GetProperties()
                .SingleOrDefault(p => p.GetGetMethod().MetadataToken == methodInfo.MetadataToken)
                ?.GetAttribute<DirectAccess>()

            if(methodInfo.)
                return Get(methodInfo, methodCallMessage);
        }

        object Get(MethodInfo methodInfo)
        {
            var className = methodInfo.DeclaringType.AssertNotNull().FullName;
            var methodName = methodInfo.Name;

            var c = new FileBasedCommunicatorClient
                (Constants.RootPath.PathCombine(ObjectURI, className, methodName));

            var resultType = methodInfo.ReturnType;
            return c
                .Get(methodInfo.MetadataToken.ToString())
                .FromJson(resultType);
        }

        object Get(MethodInfo methodInfo, IMethodCallMessage methodCallMessage)
        {
            var className = methodInfo.DeclaringType.AssertNotNull().FullName;
            var methodName = methodInfo.Name;
            var c = new FileBasedCommunicatorClient
                (Constants.RootPath.PathCombine(ObjectURI, className, methodName));

            var resultType = methodInfo.ReturnType;
            return c
                .Get
                (
                    methodCallMessage.Args.Select(a => a.ToJson()).ToArray(),
                    methodInfo.MetadataToken.ToString()
                )
                .FromJson(resultType);
        }

        ReturnMessage SyncProcessProperty
            (IMethodCallMessage message, MethodInfo method)
        {
            NotImplementedMethod(message, method);
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
    }
}