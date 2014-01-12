using System;
using System.Collections.Generic;
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

            bool allFail = false;
            string allException = null;
            
            // Class Initialize
            try
            {
                RunClassInitialize();
            }
            catch (Exception e)
            {
                allFail = true;
                allException = e.ToString();
            }

            if (!IsStaticClass)
            {
                foreach (var testMethod in TestMethods)
                {
                    if (allFail)
                    {
                        results.Add(new TestResult(testMethod.Method.Name, false, _classType.Name, allException));
                        continue;
                    }

                    if (ct.IsCancellationRequested)
                        return new List<TestResult>();

                    object instance = Activator.CreateInstance(_classType);
                    testRunner.TestInitialize(instance, testMethod.Method.Name, this);
                    
                    bool success = true;
                    string exceptionString = null;
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
                            success = false;
                            exceptionString = e.ToString();
                        }
                    }

                    if (!success)
                        continue;
                    
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
                            success = false;
                            exceptionString = ex.ToString();
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
                            success = false;
                            exceptionString = e.ToString();
                        }
                    }

                    results.Add(new TestResult(
                        testMethod.Method.Name,
                        success,
                        _classType.Name,
                        exceptionString
                    ));

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