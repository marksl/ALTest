using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AltMstest.Core;
using AltMstestGui.Configuration;

namespace altmstest.console
{
    class Program
    {
        static void Main()
        {
            var serviceConfigSection = ConfigurationManager.GetSection("FolderSection") as AltMstestSection;

            IList<ISyncedDestination> synced = FolderSync.Sync(serviceConfigSection);

            var assemblies = synced.SelectMany(a => a.AssembliesWithFullPath);
            TestRunnerLauncher.LoadAssembliesAndRunTests(assemblies);   
        }
    }
}
