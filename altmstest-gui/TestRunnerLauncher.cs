using System;
using System.Collections.Generic;
using System.IO;

namespace AltMstestGui
{
    internal class TestRunnerLauncher
    {
        public static void LoadAssembliesAndRunTests(IEnumerable<string> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var name = Path.GetFileNameWithoutExtension(assembly);
                var directoryName = Path.GetDirectoryName(assembly);
                var dirPath = Path.GetFullPath(directoryName);

                var setup = new AppDomainSetup
                                {
                                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                                    PrivateBinPath = dirPath,
                                    ShadowCopyFiles = "true",
                                    ShadowCopyDirectories = dirPath,
                                };

                AppDomain domain = AppDomain.CreateDomain(name + "Domain", AppDomain.CurrentDomain.Evidence, setup);

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