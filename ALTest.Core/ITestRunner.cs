using System.Collections.Generic;
using System.Reflection;

namespace ALTest.Core
{
    public interface ITestRunner
    {
        void AssemblyInitialize(ICollection<MethodInfo> assemblyInitialize);
        void TestInitialize(object instance, string testName, TestClass testClass);
    }
}