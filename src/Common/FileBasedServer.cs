using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Common
{
    public sealed class FileBasedServer : DumpableObject, IDisposable
    {
        readonly FileBasedCommunicatorServer Parent;
        readonly List<object> Singletons = new List<object>();

        public FileBasedServer(string uri)
        {
            Parent = new FileBasedCommunicatorServer(uri, Get);
            Parent.Start();
        }

        string Get(string className, string methodName, string value)
        {
            var pair = Singletons
                .Select
                (
                    entry => new
                    {
                        TargetObject = entry,
                        TargetType = GetTypeFromName(className, entry.GetType())
                    }
                )
                .EnsureAny(() => Tracer.Line("No matching type registered"))
                .EnsureNoDuplicate(() => Tracer.Line("More than one matching type registered"))
                .Single(entry => entry.TargetType != null);

            var args = value.Split(',');

            var targetMethod = pair
                .TargetType
                .GetMethod(methodName);

            var actualArgs = targetMethod
                .GetParameters()
                .Select((p, i) => args[i].UnescapeComma().FromJson(p.ParameterType))
                .ToArray();

            var targetObject = pair.TargetObject;

            return targetMethod.Invoke(targetObject, actualArgs).ToJson();
        }

        static Type GetTypeFromName(string className, Type objectType)
        {
            Tracer.Assert(objectType != null);
            if(objectType.FullName == className)
                return objectType;

            return objectType
                .GetInterfaces()
                .FirstOrDefault(item => item.FullName == className);
        }

        void IDisposable.Dispose() => Parent.Stop();

        public void Register(object instance) => Singletons.Add(instance);
    }
}