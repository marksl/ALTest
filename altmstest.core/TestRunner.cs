using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AltMstest.Core
{
    [Serializable]
    public class TestRunner : MarshalByRefObject
    {
        private static readonly Dictionary<string, Assembly> asses = new Dictionary<string, Assembly>();
        private static readonly object assesLock = new object();
        private CancellationTokenSource _tokenSource;

        public void Cancel()
        {
            _tokenSource.Cancel();
        }

        public ICollection<TestResult> RunTests(string assembly, bool parallel, int? degreeOfParallelism)
        {
            var results = new List<TestResult>();

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            var t = Task.Factory.StartNew(() =>
                                              {
                                                  TestRun run = GetTestRunFromAssembly(assembly, token);

                                                  var result = run.Run(parallel, degreeOfParallelism, token);
                                                  results.AddRange(result.Where(c => !c.TestPassed));
                                              },
                                          token);

            t.Wait();

            return results;
        }

        private static TestRun GetTestRunFromAssembly(string assembly, CancellationToken ct)
        {
            var run = new TestRun();

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            Assembly ass = Assembly.LoadFile(assembly);

            // Get all classes.. no abstract classes.
            var allClasses = ass.GetTypes().Where(t => t.IsClass).ToList();

            var types = allClasses.Where(t => !t.IsAbstract).ToList();

            // Add static classes
            types.AddRange(allClasses.Where(t => t.IsSealed && t.IsAbstract));

            var configFilePath = ass.Location + ".config";
            if (File.Exists(configFilePath))
            {
                run.SetConfigFile(configFilePath);
            }

            foreach (var type in types)
            {
                if (ct.IsCancellationRequested)
                    return new TestRun();

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

                        var methods = type.GetMethods().OrderBy(m => m.Name);
                        foreach (var method in methods)
                        {
                            if (ct.IsCancellationRequested)
                                return new TestRun();

                            object[] methodAttributes = method.GetCustomAttributes(false);

                            if (methodAttributes.Any(c => c as TestMethodAttribute != null))
                            {
                                var expectedException =
                                    (ExpectedExceptionAttribute) methodAttributes.FirstOrDefault(c => c as ExpectedExceptionAttribute != null);

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
            string dll = firstComma == -1
                             ? args.Name + ".dll"
                             : args.Name.Substring(0, firstComma) + ".dll";
            string exe = firstComma == -1
                             ? args.Name + ".exe"
                             : args.Name.Substring(0, firstComma) + ".exe";
            
            Assembly ass;

            lock (assesLock)
            {
                if (!asses.TryGetValue(dll, out ass))
                {
                    if (args.RequestingAssembly != null)
                    {
                        var fileInfo = new FileInfo(args.RequestingAssembly.Location);
                        var fullPath = Path.Combine(fileInfo.DirectoryName, dll);
                        if (File.Exists(fullPath))
                        {
                            ass = Assembly.LoadFile(fullPath);
                        }
                        else
                        {
                            fullPath = Path.Combine(fileInfo.DirectoryName, exe);
                            if (File.Exists(fullPath))
                            {
                                ass = Assembly.LoadFile(fullPath);
                            }
                        }
                    }

                    // TODO: This should use the Assembly hash as part of the key
                    asses.Add(dll, ass);
                }
            }

            return ass;
        }    }
}