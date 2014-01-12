using System;
using System.Configuration;
using ALTest.Core;
using ALTest.Core.Configuration;

namespace ALTest.Console
{
    internal class Program
    {
        private static void Main()
        {
            var serviceConfigSection = ConfigurationManager.GetSection("AssemblySection") as ALTestSection;
            if (serviceConfigSection == null)
                throw new MissingFieldException("AssemblySection is missing in App.config.");

            var destination = serviceConfigSection.Destination;
            var assemblyList = serviceConfigSection.AssemblyList;
            var testAssembly = serviceConfigSection.TestAssembly;

            var runner = new TestRunner();
            var t = runner.Start(DateTime.Now, destination, assemblyList, testAssembly);
            t.Wait();
        }
    }
}