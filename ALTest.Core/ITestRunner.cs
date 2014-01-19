using System;
using System.Collections.Generic;
using System.Reflection;

namespace ALTest.Core
{
    public interface ITestRunner
    {
        void AssemblyInitialize(ICollection<MethodInfo> assemblyInitialize);
        void TestInitialize(object instance, string testName, TestClass testClass);
        void WriteResults(DateTime start, DateTime finish,
                          ICollection<TestResult> results,
                          Dictionary<string, ICollection<TestResult>> resultsGroupedByAssembly,
                          string fileName);
    }
}