using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ALTest.Core;

namespace ALTest.Xunit
{
    public class XunitTestClass : TestClass
    {
        private readonly Type _classType;
        public XunitTestClass(Type classType)
            : base(classType)
        {
            fixtures = new Dictionary<Type, Tuple<MethodInfo, object>>();
            _classType = classType;
        }

        public void AddFixture(Type fixtureType)
        {
            fixtures.Add(fixtureType, null);
            fff.Add(fixtureType);
        }


        // Invoke SetFixture for each of the IUseFixture<> interfaces.
        // This is done for each test intialize
        public void InvokeTestInitialize(object instance, string testName)
        {
            foreach (var f in fixtures)
            {
                f.Value.Item1.Invoke(instance, new[] {f.Value.Item2});
            }
        }

        //public override List<TestResult> Run(System.Threading.CancellationToken ct, ITestRunner testRunner)
        //{
        //    // TODO: Probably have to pretty much implement this whole thing... again.
        //    throw new NotImplementedException();   
        //}

        private readonly Dictionary<Type, Tuple<MethodInfo, object>> fixtures;

        protected override void RunClassInitialize()
        {
            Type[] keyCollection = fixtures.Keys.Select(x => x).ToArray();
            foreach (var t in keyCollection)
            {
                var instance = Activator.CreateInstance(t);
                var methodInfo = _classType.GetMethod("SetFixture", new[] {t});
                fixtures[t] = new Tuple<MethodInfo, object>(methodInfo, instance);
            }
        }

        protected override void RunClassCleanup()
        {
            object[] valueCollection = fixtures.Values.Select(x=>x.Item2).ToArray();
            foreach (var instance in valueCollection)
            {
                var disposable = instance as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }
    }
}