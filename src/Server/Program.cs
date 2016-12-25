using System;
using System.Linq;
using System.Runtime.Remoting;
using Common;
using hw.DebugFormatter;
using hw.Helper;


namespace Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            var console = Console.Out;
            Tracer.LinePart("");
            Console.SetOut(console);
            DebugTextWriter.Register(false);

            var server = new FileBasedServer(Constants.RootPath.PathCombine("Mmasf"));

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(TestData), "", WellKnownObjectMode.Singleton);

            server.Start();
            "(Server)Press any key:".WriteLine();

            var k = Console.ReadKey();
        }
    }

    class FileBasedServer
    {
        readonly FileBasedCommunicatorServer Parent;
        readonly FunctionCache<Type, object> Singletons = new FunctionCache<Type, object>(Activator.CreateInstance);
        public FileBasedServer(string directory) { Parent = new FileBasedCommunicatorServer(directory, Get); }

        string Get(string name, string value)
        {
            var x =
                RemotingConfiguration
                    .GetRegisteredWellKnownServiceTypes()
                    .Single(i => i.ObjectType.GetInterfaces().Any(item => item.FullName == name));

            var inType = x.ObjectType.GetInterface(name);
            var ob = x.Mode == WellKnownObjectMode.SingleCall
                ? Activator.CreateInstance(x.ObjectType)
                : Singletons[x.ObjectType];

            var method = inType.GetMethod(value);
            var result = method.Invoke(ob, null);


            Dumpable.NotImplementedFunction(name, value);
            return null;
        }

        public void Start() { Parent.Start(); }
    }

    class TestData : ITestData
    {
        string[] ITestData.TestArray { get { throw new NotImplementedException(); } }

        int ITestData.TestInt { get { throw new NotImplementedException(); } }

        string ITestData.TestString => "TestStringValue";
    }
}