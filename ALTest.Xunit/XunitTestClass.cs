using System;
using System.Collections.Generic;
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

        private readonly Dictionary<Type, Tuple<MethodInfo, object>> fixtures;

        protected override void RunClassInitialize()
        {
            foreach (var t in fixtures.Keys)
            {
                var instance = Activator.CreateInstance(t);
                var methodInfo = _classType.GetMethod("SetFixture", new[] {t});
                fixtures[t] = new Tuple<MethodInfo, object>(methodInfo, instance);
            }
        }

        protected override void RunClassCleanup()
        {
            foreach (var tuple in fixtures.Values)
            {
                var instance = tuple.Item2;
                var disposable = instance as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }
    }
}