using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace ALTest.Core
{
    public abstract class TestClass
    {
        protected TestClass(Type classType)
        {
            _classType = classType;

            var types = new List<Type>();

            Type t = _classType;
            while (t != null)
            {
                types.Add(t);

                t = t.BaseType;
            }

            typeSortedIndex = new Dictionary<string, int>();
            typeReverseSortedIndex = new Dictionary<string, int>();
            for (int i = 0; i < types.Count; i ++)
            {
                string name = types[i].Name;
                typeSortedIndex[name] = i;
                typeReverseSortedIndex[name] = types.Count - i;
            }

            ClassInitialize = new List<MethodInfo>();
            ClassCleanup = new List<MethodInfo>();
            TestInitialize = new List<MethodInfo>();
            TestCleanup = new List<MethodInfo>();
            TestMethods = new List<TestMethod>();
        }

        private readonly Dictionary<string, int> typeSortedIndex;
        private readonly Dictionary<string, int> typeReverseSortedIndex;
        private readonly Type _classType;

        public List<MethodInfo> ClassInitialize { get; set; }
        public List<MethodInfo> ClassCleanup { get; set; }

        public List<MethodInfo> TestInitialize { get; set; }
        public List<MethodInfo> TestCleanup { get; set; }

        public List<TestMethod> TestMethods { get; set; }
        
        public delegate object MyMethodInvoker(object obj);

        public TestMethod AddMethodTestrun(MethodInfo method, Type expectedException)
        {
            var run = new TestMethod
                          {
                              Method = method,
                              ExpectedExceptionType = expectedException
                          };

            TestMethods.Add(run);

            return run;
        }

        // ReSharper disable PossibleNullReferenceException
        private Comparison<MethodInfo> SortSubClassesLast()
        {
            return (a, b) =>
                   typeReverseSortedIndex[a.DeclaringType.Name].CompareTo(
                       typeReverseSortedIndex[b.DeclaringType.Name]);
        }

        private Comparison<MethodInfo> SortSubClassesFirst()
        {
            return (a, b) =>
                   typeSortedIndex[a.DeclaringType.Name].CompareTo(
                       typeSortedIndex[b.DeclaringType.Name]);
        }
        // ReSharper restore PossibleNullReferenceException

        // TODO: Need to actually test this
        // Need to initialize base classes first.
        // Need to cleanup derived classes first.
        public void SortMethods()
        {
            ClassInitialize.Sort(SortSubClassesLast()); 
            ClassCleanup.Sort(SortSubClassesFirst());

            TestInitialize.Sort(SortSubClassesLast());
            ClassCleanup.Sort(SortSubClassesFirst());
        }

        bool IsStaticClass
        {
            get { return _classType.IsSealed && _classType.IsAbstract; }
        }

        protected abstract void RunClassInitialize();

        public List<TestResult> Run(CancellationToken ct, ITestRunner testRunner)
        {
            var results = new List<TestResult>(100);

            if (ct.IsCancellationRequested)
                return results;

            Exception allException = null;

            DateTime start = DateTime.Now;
            var watch = Stopwatch.StartNew();

            // Class Initialize
            try
            {
                RunClassInitialize();
            }
            catch (Exception e)
            {
                allException = e;
            }

            if (allException != null)
            {
                foreach (var testMethod in TestMethods)
                {
                    AddResult(results, testMethod, allException, start, watch);
                }

                return results;
            }

            if (!IsStaticClass)
            {
                foreach (var testMethod in TestMethods)
                {
                    if (ct.IsCancellationRequested)
                        return new List<TestResult>();

                    object instance = Activator.CreateInstance(_classType);
                    testRunner.TestInitialize(instance, testMethod.Method.Name, this);
                    
                    Exception exception = null;
                    // Test Initialize
                    foreach (var testInit in TestInitialize)
                    {
                        Action a = CreateMethod(instance, testInit);

                        try
                        {
                            RunMethod(a);
                        }
                        catch (Exception e)
                        {
                            exception = e;
                            break;
                        }
                    }

                    if (exception != null)
                    {
                        AddResult(results, testMethod, exception, start, watch);
                        continue;
                    }
                    
                    try
                    {
                        Action a = CreateMethod(instance, testMethod.Method);

                        RunMethod(a);
                    }
                    catch (Exception ex)
                    {
                        if (testMethod.ExpectedExceptionType != null &&
                            testMethod.ExpectedExceptionType == ex.GetType())
                        {
                        }
                        else
                        {
                            exception = ex;
                        }
                    }

                    // Test Cleanup
                    foreach (var testCleanup in TestCleanup)
                    {
                        Action a = CreateMethod(instance, testCleanup);

                        try
                        {
                            RunMethod(a);
                        }
                        catch (Exception e)
                        {
                            exception = e;
                            break;
                        }
                    }

                    AddResult(results, testMethod, exception, start, watch);
                }
            }

            // Class Cleanup
            foreach (var classCleanup in ClassCleanup)
            {
                try
                {
                    classCleanup.Invoke(null, null);
                }
                catch { }
            }

            return results;
        }

        private void AddResult(ICollection<TestResult> results, TestMethod testMethod, Exception exception, 
            DateTime start, Stopwatch watch)
        {
            StdOut.Write("{0,-22}", exception == null ? "Passed" : "Failed");
            StdOut.WriteLine("{0}.{1}", _classType.FullName, testMethod.Method.Name);

            var testResult = new TestResult(testMethod.Method.Name, exception == null, _classType.FullName, exception)
                                 {
                                     Duration = watch.Elapsed,
                                     StartTime = start,
                                     EndTime = DateTime.Now
                                 };
            results.Add(testResult);

            watch.Restart();
        }

        private static void RunMethod(Action a)
        {
            a();
        }

        private static Action CreateMethod(object instance, MethodInfo testMethod)
        {
            return (Action)Delegate.CreateDelegate(typeof(Action), instance, testMethod, true);
        }
    }
}