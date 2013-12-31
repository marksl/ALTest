using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AltMstestGui
{
    public class ClassTestRun
    {
        public ClassTestRun(Type classType)
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
            TestMethods = new List<MethodTestRun>();
        }

        private readonly Dictionary<string, int> typeSortedIndex;
        private readonly Dictionary<string, int> typeReverseSortedIndex;
        private readonly Type _classType;

        public PropertyInfo TestContextMethod { get; set; }

        public List<MethodInfo> ClassInitialize { get; set; }
        public List<MethodInfo> ClassCleanup { get; set; }

        public List<MethodInfo> TestInitialize { get; set; }
        public List<MethodInfo> TestCleanup { get; set; }

        public List<MethodTestRun> TestMethods { get; set; }

        public MethodTestRun AddMethodTestrun(MethodInfo method, ExpectedExceptionAttribute expectedException)
        {
            var run = new MethodTestRun
                          {
                              Method = method,
                              ExpectedException = expectedException
                          };

            TestMethods.Add(run);

            return run;
        }

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

        // Need to initialize base classes first.
        // Need to cleanup derived classes first.
        public void SortMethods()
        {
            ClassInitialize.Sort(SortSubClassesLast()); 
            ClassCleanup.Sort(SortSubClassesFirst());

            TestInitialize.Sort(SortSubClassesLast());
            ClassCleanup.Sort(SortSubClassesFirst());
        }

        public List<TestResult> Run(MyTestContext context)
        {
            var results = new List<TestResult>(100);

            // Class Initialize
            foreach (var classInit in ClassInitialize)
            {
                classInit.Invoke(null, new object[] {context});
            }

            object instance = Activator.CreateInstance(_classType);

            // Initialize the context
            if (TestContextMethod != null)
            {
                TestContextMethod.SetValue(instance, instance, null);
            }

            foreach (var testMethod in TestMethods)
            {
                context.Properties["TestName"] = testMethod.Method.Name;
                
                // Test Initialize
                foreach (var testInit in TestInitialize)
                {
                    testInit.Invoke(instance, null);
                }

                // TODO... Need to capture the stack trace... hmmm this might be an optional setting.
                bool success = false;
                try
                {
NEXT STEP. Just run the tests and see what passes and what fails!!!

                    // Oh boy. Here's the actual execution of the test!!
                    testMethod.Method.Invoke(instance, null);
                    success = true;
                }
                catch (AssertFailedException)
                {
                    success = false;
                }
                catch (Exception ex)
                {
                    if (testMethod.ExpectedException != null &&
                        testMethod.ExpectedException.ExceptionType == ex.GetType())
                    {
                        success = true;
                    }
                    else
                    {
                        // TODO: Figure out what these mean..
                        success = false;
                    }
                }

                results.Add(new TestResult()
                                {
                                    TestName = testMethod.Method.Name,
                                    TestPassed = success
                                });

                // Test Cleanup
                foreach (var testCleanup in TestCleanup)
                {
                    testCleanup.Invoke(instance, new object[] { context });
                }
            }

            // Class Cleanup
            foreach (var classCleanup in ClassCleanup)
            {
                classCleanup.Invoke(null, new object[] {context});
            }

            return results;
        }
    }
}