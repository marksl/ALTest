﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Fasterflect;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AltMstest.Core
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
        
        public delegate object MyMethodInvoker(object obj);

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

        public List<TestResult> Run(CancellationToken ct)
        {
            var results = new List<TestResult>(100);

            if (ct.IsCancellationRequested)
                return results;

            bool allFail = false;
            // Class Initialize
            foreach (var classInit in ClassInitialize)
            {
                try
                {
                    classInit.Invoke(null, new object[] {new MyTestContext()});
                }
                catch
                {
                    allFail = true;
                }
            }

            if (!IsStaticClass)
            {
                foreach (var testMethod in TestMethods)
                {
                    if (allFail)
                    {
                        results.Add(new TestResult
                        {
                            TestName = testMethod.Method.Name,
                            TestPassed = false
                        });
                        continue;
                    }

                    if (ct.IsCancellationRequested)
                        return new List<TestResult>();


                    var ignore = testMethod.Method.GetCustomAttributes(typeof (IgnoreAttribute), false);
                    if (ignore.Length > 0)
                    {
                        continue;
                    }

                    var context = new MyTestContext();
                    object instance = Activator.CreateInstance(_classType);

                    // Initialize the context
                    if (TestContextMethod != null)
                    {
                        TestContextMethod.SetValue(instance, context, null);
                    }
                    
                    context.Properties["TestName"] = testMethod.Method.Name;

                    bool success = true;
                    string stackTrace = null;
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
                            stackTrace = e.StackTrace;
                        }
                        
                        //testInit.Call(instance);
                    }

                    if (!success)
                        continue;

                    
                    try
                    {
                        //testMethod.Method.Call(instance);


                        Action a = CreateMethod(instance, testMethod.Method);

                        RunMethod(a);
                    }
                    catch (AssertFailedException e)
                    {
                        success = false;
                        stackTrace = e.StackTrace;
                    }
                    catch (Exception ex)
                    {
                        if (testMethod.ExpectedException != null &&
                            testMethod.ExpectedException.ExceptionType == ex.GetType())
                        {
                        }
                        else
                        {
                            success = false;
                            stackTrace = ex.StackTrace;
                        }
                    }

                    // Test Cleanup
                    foreach (var testCleanup in TestCleanup)
                    {
                        //testCleanup.Call(instance);

                        Action a = CreateMethod(instance, testCleanup);

                        try
                        {
                            RunMethod(a);
                        }
                        catch (Exception e)
                        {
                            success = false;
                            stackTrace = e.StackTrace;
                        }
                    }

                    results.Add(new TestResult
                    {
                        ClassName = _classType.Name,
                        TestName = testMethod.Method.Name,
                        TestPassed = success,
                        StackTrace = stackTrace
                    });

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