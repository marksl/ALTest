using System;
using System.Collections.Generic;
using System.Reflection;
using ALTest.Core;

namespace ALTest.MsTest
{
    public class MsTestRunner : ITestRunner
    {
        public void AssemblyInitialize(ICollection<MethodInfo> assemblyInitialize)
        {
            foreach (var assemblyInit in assemblyInitialize)
            {
                assemblyInit.Invoke(null, new object[] { new MsTestContext() });
            }
        }

        public PropertyInfo TestContextMethod { get; set; }

        public void TestInitialize(object instance, string testName, TestClass testClass)
        {
            var context = new MsTestContext();
            context.Properties["TestName"] = testName;

            TestContextMethod = ((MsTestClass) testClass).TestContextMethod;

            // Initialize the context
            if (TestContextMethod != null)
            {
                TestContextMethod.SetValue(instance, context, null);
            }
        }

        public void WriteResults(DateTime start, DateTime finish, 
            ICollection<TestResult> results, 
            Dictionary<string, ICollection<TestResult>> assemblyResults, 
            string fileName)
        {
            MsTestResultsFile.WriteResults(start, finish, results, assemblyResults, fileName);
        }
    }
}