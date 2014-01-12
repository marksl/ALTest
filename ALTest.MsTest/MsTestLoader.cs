using System;
using System.Linq;
using System.Reflection;
using ALTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALTest.MsTest
{
    public class MsTestLoader : ITestLoader
    {
        public void Load(Type type, ITestAssembly assembly)
        {
            if (type.GetCustomAttributes(typeof(TestClassAttribute), false).Length > 0)
            {
                var testClass = new MsTestClass(type);
                assembly.AddTestClass(testClass);

                PropertyInfo prop = type.GetProperty("TestContext", BindingFlags.FlattenHierarchy);
                if (prop != null && prop.PropertyType == typeof(TestContext))
                {
                    testClass.TestContextMethod = prop;
                }

                var methods = type.GetMethods().OrderBy(m => m.Name);
                foreach (var method in methods)
                {
                    object[] methodAttributes = method.GetCustomAttributes(false);

                    if (methodAttributes.Any(c => c as TestMethodAttribute != null)
                        && methodAttributes.All(c => c as IgnoreAttribute == null))
                    {
                        var expectedException =
                            (ExpectedExceptionAttribute)methodAttributes.FirstOrDefault(c => c as ExpectedExceptionAttribute != null);

                        testClass.AddMethodTestrun(method, expectedException != null ? expectedException.ExceptionType : null);
                    }

                    if (methodAttributes.Any(c => c as AssemblyInitializeAttribute != null))
                    {
                        assembly.AddAssemblyInitialize(method);
                    }

                    if (methodAttributes.Any(c => c as AssemblyCleanupAttribute != null))
                    {
                        assembly.AddAssemblyCleanup(method);
                    }

                    if (methodAttributes.Any(c => c as TestInitializeAttribute != null))
                    {
                        testClass.TestInitialize.Add(method);
                    }

                    if (methodAttributes.Any(c => c as TestCleanupAttribute != null))
                    {
                        testClass.TestCleanup.Add(method);
                    }

                    if (methodAttributes.Any(c => c as ClassInitializeAttribute != null))
                    {
                        testClass.ClassInitialize.Add(method);
                    }

                    if (methodAttributes.Any(c => c as ClassCleanupAttribute != null))
                    {
                        testClass.ClassCleanup.Add(method);
                    }
                }

                testClass.SortMethods();
            }
        }
    }
}