using System.Reflection;

namespace ALTest.Core
{
    public interface ITestAssembly
    {
        void AddAssemblyInitialize(MethodInfo method);
        void AddAssemblyCleanup(MethodInfo method);
        void AddTestClass(TestClass testClass);
    }
}