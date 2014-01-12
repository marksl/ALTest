using System;

namespace ALTest.Core
{
    public interface ITestLoader
    {
        void Load(Type type, ITestAssembly assembly);
    }
}