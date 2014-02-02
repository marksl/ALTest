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

            var runWith = (RunWithAttribute) type.GetCustomAttributes(typeof (RunWithAttribute), true).FirstOrDefault();
            if (runWith != null)
            {
                testClass.RunWith = runWith;

            }

            bool idisposable = type.IsAssignableFrom(typeof (IDisposable));
            MethodInfo testCleanup = idisposable ? type.GetMethod("Dispose") : null;

            var fixtures = type.GetInterfaces()
                  .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IUseFixture<>)).ToList();
            foreach (var fixture in fixtures)
            {
                var args = fixture.GetGenericArguments();
                testClass.AddFixture(args[0]);
            }

            foreach (var method in methods)//.Where(x => x.Name == "Can_setup_decorator_pattern"))
            {
                object[] methodAttributes = method.GetCustomAttributes(true);

                var fact = (FactAttribute) methodAttributes.FirstOrDefault(c => c as FactAttribute != null);
                if (fact != null && fact.Skip == null)
                {
                    //testClass.AddMethodTestrun(method, null);
                    testClass.XunitMethods.Add(
                        new XunitTestMethod
                            {
                                Fact = fact,
                                Method = method
                                }
                        );
                }

                if (testCleanup != null)
                {
                    testClass.ClassCleanup.Add(testCleanup);
                }
            }

            testClass.SortMethods();
        }
    }
}