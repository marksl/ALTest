using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AltMstestGui
{
    [Serializable]
    public class TestResult : MarshalByRefObject
    {
        public string TestName { get; set; }
        public bool TestPassed { get; set; }
    }

    public class TestRun
    {
        public TestRun()
        {
            Classes = new List<ClassTestRun>();
            AssemblyInitialize = new List<MethodInfo>();
            AssemblyCleanup = new List<MethodInfo>();
        }

        public ClassTestRun AddClassTestRun(Type classType)
        {
            var run = new ClassTestRun(classType);
            Classes.Add(run);

            return run;
        }

        public List<TestResult> Run()
        {
            var results = new List<TestResult>();
           
            var context = new MyTestContext();

            // Assembly initialize
            foreach (var assemblyInit in AssemblyInitialize)
            {
                assemblyInit.Invoke(null, new object[] {context});
            }
            

            // Run tests for each class.
            foreach (List<TestResult> classResults in Classes.Select(c => c.Run(context)))
            {
                results.AddRange(classResults);
            }

            // Assembly cleanup
            foreach (var assemblyInit in AssemblyCleanup)
            {
                assemblyInit.Invoke(null, null);
            }

            return results;
        }

        // Apparently only one Assembly Initialize is supported... TODO: Test if you can have more than 1.
        public List<MethodInfo> AssemblyInitialize { get; set; }
        public List<MethodInfo> AssemblyCleanup { get; set; }

        public List<ClassTestRun> Classes { get; private set; }
    }
}