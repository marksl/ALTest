using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ALTest.Core
{
    [Serializable]
    public class TestAssembly : MarshalByRefObject, ITestAssembly
    {
        public List<MethodInfo> AssemblyInitialize { get; set; }
        public List<MethodInfo> AssemblyCleanup { get; set; }
        public List<TestClass> Classes { get; private set; }

        private static readonly Dictionary<string, Assembly> asses = new Dictionary<string, Assembly>();
        private static readonly object assesLock = new object();
        private CancellationTokenSource _tokenSource;

        private ITestLoader _testLoader;
        private ITestRunner _testRunner;

        public TestAssembly()
        {
            Classes = new List<TestClass>();
            AssemblyInitialize = new List<MethodInfo>();
            AssemblyCleanup = new List<MethodInfo>();
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }

        public ICollection<TestResult> RunTests(string assembly, bool parallel, int? degreeOfParallelism, string testAssembly, out int testsRan)
        {
            var failures = new List<TestResult>();
            int num = 0;
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            var t = Task.Factory.StartNew(() =>
                                              {
                                                  ITestFactory factory = TestFactoryLoader.Load(testAssembly);
                                                  _testLoader = factory.CreateTestLoader();
                                                  _testRunner = factory.CreateTestRunner();
                                                  
                                                  if (TryLoadTestsFromAssembly(assembly, token))
                                                  {
                                                      var result = Run(parallel, degreeOfParallelism, token);
                                                      failures.AddRange(result.Where(c => !c.TestPassed));

                                                      num = result.Count;
                                                  }
                                              },
                                          token);

            t.Wait();

            testsRan = num;

            return failures;
        }

        private string _configFile;
        public void SetConfigFile(string configFilePath)
        {
            _configFile = configFilePath;
        }

        private bool TryLoadTestsFromAssembly(string assembly, CancellationToken ct)
        {
            StdOut.WriteLine("Loading {0}...", assembly);

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            Assembly ass = Assembly.LoadFile(assembly);

            // Get all classes.. no abstract classes.
            var allClasses = ass.GetTypes().Where(t => t.IsClass).ToList();

            List<Type> types = allClasses.Where(t => !t.IsAbstract).ToList();

            // Add static classes
            types.AddRange(allClasses.Where(t => t.IsSealed && t.IsAbstract));

            var configFilePath = ass.Location + ".config";
            if (File.Exists(configFilePath))
            {
                SetConfigFile(configFilePath);
            }

            foreach (Type type in types)
            {
                if (ct.IsCancellationRequested)
                    return false;

                _testLoader.Load(type, this);
            }

            return true;
        }

        public List<TestResult> Run(bool parallel, int? degreeOfParallelism, CancellationToken ct)
        {
            StdOut.WriteLine("Starting execution...");
            StdOut.WriteLine();

            StdOut.Write("{0,-22}","Results");
            StdOut.WriteLine("{0}", "Top Level Tests");
            StdOut.Write("{0,-22}", "-------");
            StdOut.WriteLine("{0}", "---------------");

            var results = new List<TestResult>();

            using (AppConfig.Change(_configFile))
            {
                try
                {
                    _testRunner.AssemblyInitialize(AssemblyInitialize);
                }
                catch (Exception e)
                {
                    results.Add(new TestResult("Assembly Initialized failed.", false, "Assembly", e.ToString()));

                    return results;
                }

                if (parallel)
                {
                    var l = new object();

                    ParallelQuery<TestClass> classTestRuns = Classes.AsParallel();
                    if (degreeOfParallelism.HasValue)
                    {
                        classTestRuns = classTestRuns.WithDegreeOfParallelism(degreeOfParallelism.Value);
                    }

                    foreach (List<TestResult> classResults in classTestRuns.Select(c => c.Run(ct, _testRunner)))
                    {
                        lock (l)
                        {
                            results.AddRange(classResults);
                        }
                    }
                }
                else
                {
                    foreach (List<TestResult> classResults in Classes.Select(c => c.Run(ct, _testRunner)))
                    {
                        results.AddRange(classResults);
                    }
                }

                // Assembly cleanup
                foreach (var assemblyInit in AssemblyCleanup)
                {
                    assemblyInit.Invoke(null, null);
                }
            }

            return results;
        }


        protected virtual Assembly AssemblyResolve(object sender, ResolveEventArgs args)
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

                    asses.Add(dll, ass);
                }
            }

            return ass;
        }

        public void AddAssemblyInitialize(MethodInfo method)
        {
            AssemblyInitialize.Add(method);
        }

        public void AddAssemblyCleanup(MethodInfo method)
        {
            AssemblyCleanup.Add(method);
        }

        public void AddTestClass(TestClass testClass)
        {
            Classes.Add(testClass);
        }
    }
}