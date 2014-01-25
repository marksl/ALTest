using System;
using System.Collections.Generic;
using System.Reflection;
using ALTest.Core;

namespace ALTest.Xunit
{
    public class XunitTestRunner : ITestRunner
    {
        public void AssemblyInitialize(ICollection<MethodInfo> assemblyInitialize)
        {
        }

        public void TestInitialize(object instance, string testName, TestClass testClass)
        {
            ((XunitTestClass)testClass).InvokeTestInitialize(instance, testName);
        }

        public void WriteResults(DateTime start, DateTime finish, ICollection<TestResult> results, Dictionary<string, ICollection<TestResult>> resultsGroupedByAssembly, string fileName)
        {
            // TODO: Separate this out.
        }
    }
}