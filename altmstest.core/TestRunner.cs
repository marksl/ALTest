using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AltMstestGui
{
    [Serializable]
    public class TestRunner : MarshalByRefObject
    {
        public void RunTests(string assembly)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            TestRun run = GetTestRunFromAssembly(assembly);

            var result = run.Run();
            stopWatch.Stop();
            if (result != null)
            {
            }
        }

        private static TestRun GetTestRunFromAssembly(string assembly)
        {
            var run = new TestRun();

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            Assembly ass = Assembly.LoadFile(assembly);

            // Get all classes.. no abstract classes.
            var allClasses = ass.GetTypes().Where(t => t.IsClass).ToList();

            var types = allClasses.Where(t=>!t.IsAbstract).ToList();

            // Add static classes
            types.AddRange(allClasses.Where(t=>t.IsSealed && t.IsAbstract));

            foreach (var type in types)
            {
                if (type != null)
                {
                    if (type.GetCustomAttributes(typeof (TestClassAttribute), false).Length > 0)
                    {
                        ClassTestRun classTestRun = run.AddClassTestRun(type);

                        PropertyInfo prop = type.GetProperty("TestContext", BindingFlags.FlattenHierarchy);
                        if (prop != null && prop.PropertyType == typeof (TestContext))
                        {
                            classTestRun.TestContextMethod = prop;
                        }

                        var methods = type.GetMethods();
                        foreach (var method in methods)
                        {
                            object[] methodAttributes = method.GetCustomAttributes(false);

                            if (methodAttributes.Any(c => c as TestMethodAttribute != null))
                            {
                                var expectedException = (ExpectedExceptionAttribute) methodAttributes.FirstOrDefault(c => c as ExpectedExceptionAttribute != null);

                                classTestRun.AddMethodTestrun(method, expectedException);
                            }

                            if (methodAttributes.Any(c => c as AssemblyInitializeAttribute != null))
                            {
                                run.AssemblyInitialize.Add(method);
                            }

                            if (methodAttributes.Any(c => c as AssemblyCleanupAttribute != null))
                            {
                                run.AssemblyCleanup.Add(method);
                            }

                            if (methodAttributes.Any(c => c as TestInitializeAttribute != null))
                            {
                                classTestRun.TestInitialize.Add(method);
                            }

                            if (methodAttributes.Any(c => c as TestCleanupAttribute != null))
                            {
                                classTestRun.TestCleanup.Add(method);
                            }

                            if (methodAttributes.Any(c => c as ClassInitializeAttribute != null))
                            {
                                classTestRun.ClassInitialize.Add(method);
                            }

                            if (methodAttributes.Any(c => c as ClassCleanupAttribute != null))
                            {
                                classTestRun.ClassCleanup.Add(method);
                            }
                        }

                        classTestRun.SortMethods();
                    }
                }
            }

            return run;
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int firstComma = args.Name.IndexOf(',');

            string dll = args.Name.Substring(0, firstComma) + ".dll";

            var fileInfo = new FileInfo(args.RequestingAssembly.Location);

            var fullPath = Path.Combine(fileInfo.DirectoryName, dll);
            return Assembly.LoadFile(fullPath);
        }
    }
}
