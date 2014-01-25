using System;
using System.Linq;
using System.Reflection;
using ALTest.Core;
using Xunit;

namespace ALTest.Xunit
{
    public class XunitTestLoader : ITestLoader
    {
        public void Load(Type type, ITestAssembly assembly)
        {
            var testClass = new XunitTestClass(type);
            assembly.AddTestClass(testClass);
            var methods = type.GetMethods().OrderBy(m => m.Name);

            bool idisposable = type.IsAssignableFrom(typeof (IDisposable));
            MethodInfo testCleanup = idisposable ? type.GetMethod("Dispose") : null;

            var fixtures = type.GetInterfaces()
                  .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IUseFixture<>)).ToList();
            foreach (var fixture in fixtures)
            {
                var args = fixture.GetGenericArguments();
                testClass.AddFixture(args[0]);
            }

            foreach (var method in methods)
            {
                object[] methodAttributes = method.GetCustomAttributes(false);

                var fact = (FactAttribute) methodAttributes.FirstOrDefault(c => c as FactAttribute != null);
                if (fact != null && fact.Skip == null)
                {
                    testClass.AddMethodTestrun(method, null);
                }

                if (testCleanup != null)
                {
                    testClass.TestCleanup.Add(testCleanup);
                }
            }

            testClass.SortMethods();
        }
    }
}