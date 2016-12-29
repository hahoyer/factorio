using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using hw.DebugFormatter;
using hw.Helper;

namespace Common
{
    public sealed class FileBasedServer : DumpableObject, IDisposable
    {
        readonly FileBasedCommunicatorServer Parent;
        readonly FunctionCache<Type, object> Singletons = new FunctionCache<Type, object>
            (Activator.CreateInstance);

        public FileBasedServer(string uri)
        {
            Parent = new FileBasedCommunicatorServer(uri, Get);
            Parent.Start();
        }

        string Get(string className, string methodName, string value)
        {
            var entries = RemotingConfiguration.GetRegisteredWellKnownServiceTypes();
            var typeEntries = entries
                .Where(i => i.ObjectType.GetInterfaces().Any(item => item.FullName == className));
            var serviceTypeEntry = typeEntries.Single();

            var args = value.Split(',');
            var objectType = serviceTypeEntry.ObjectType;
            var actualArgs = objectType
                .GetInterface(className)
                .GetMethod(methodName)
                .GetParameters()
                .Select((p, i) => args[i].UnescapeComma().FromJson(p.ParameterType))
                .ToArray();

            var ob = serviceTypeEntry.Mode == WellKnownObjectMode.SingleCall
                ? Activator.CreateInstance(objectType)
                : Singletons[objectType];

            return objectType
                .GetInterface(className)
                .GetMethod(methodName).Invoke(ob, actualArgs).ToJson();
        }

        void IDisposable.Dispose() { Parent.Stop(); }
    }
}