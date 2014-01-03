using System;
using System.Collections.Generic;
using System.IO;

namespace AltMstest.Core
{
    public class TestRunnerLauncher
    {
        public static void LoadAssembliesAndRunTests(IEnumerable<string> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var name = Path.GetFileNameWithoutExtension(assembly);

                AppDomain domain = AppDomain.CreateDomain(name + "Domain", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);

                var scannerType = typeof(TestRunner);
                var testRunner = (TestRunner)domain.CreateInstanceAndUnwrap(scannerType.Assembly.FullName, scannerType.FullName);

                testRunner.RunTests(assembly);

                AppDomain.Unload(domain);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}