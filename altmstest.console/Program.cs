using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AltMstest.Core;
using AltMstest.Core.Configuration;

namespace altmstest.console
{
    internal class Program
    {
        private static void Main()
        {
            var serviceConfigSection = ConfigurationManager.GetSection("AssemblySection") as AltMstestSection;
            var destination = serviceConfigSection.Destination;
            var assemblyList = serviceConfigSection.AssemblyList;

            var runner = new TestRunnerLauncher();
            var t = runner.Start(DateTime.Now, destination, assemblyList);
            t.Wait();
        }
    }
}