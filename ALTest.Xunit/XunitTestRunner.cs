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
            throw new NotSupportedException();
        }
    }
}