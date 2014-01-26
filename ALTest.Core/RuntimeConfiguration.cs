using System;
using System.Collections.Generic;
using ALTest.Core.Configuration;

namespace ALTest.Core
{
    [Serializable]
    public class RuntimeConfiguration
    {
        public string Destination { get; private set; }
        public IList<AssemblyConfigElement> AssemblyList { get; private set; }
        public string TestAssembly { get; private set; }

        public RuntimeConfiguration(string destination, IList<AssemblyConfigElement> assemblyList, string testAssembly, bool silent)
        {
            Destination = destination;
            AssemblyList = assemblyList;
            TestAssembly = testAssembly;

            StdOut.Silent = silent;
        }
    }
}