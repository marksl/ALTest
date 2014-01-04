using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AltMstest.Core;
using AltMstest.Core.Configuration;

namespace altmstest.console
{
    class Program
    {
        static void Main()
        {
            var serviceConfigSection = ConfigurationManager.GetSection("AssemblySection") as AltMstestSection;

            IList<ISyncedDestination> synced = FolderSync.Sync(serviceConfigSection.Destination, serviceConfigSection.AssemblyList);

            // This will likely always take the full paths of the assemblies.

            var assemblies = synced.SelectMany(a => a.AssembliesWithFullPath);
            TestRunnerLauncher.LoadAssembliesAndRunTests(assemblies);   
        }
    }
}
