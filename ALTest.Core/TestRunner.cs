using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ALTest.Core.Configuration;
using ALTest.Core.FileSynchronization;

namespace ALTest.Core
{
    public class TestRunner
    {
        public EventHandler<TestRunnerFinishedEventArgs> Finished;

        private TestAssembly _testAssembly;
        private CancellationTokenSource _tokenSource;

        public void Cancel()
        {
            _tokenSource.Cancel();

            if (_testAssembly != null)
            {
                _testAssembly.Cancel();
            }
        }

        private TestRunResult LoadAssembliesAndRunTests(IEnumerable<AssemblyInfo> assemblies, CancellationToken ct, RuntimeConfiguration configuration)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            DateTime start = DateTime.UtcNow;

            var assemblyResults = new Dictionary<string, ICollection<TestResult>>();
            foreach (AssemblyInfo assembly in assemblies)
            {
                if (ct.IsCancellationRequested)
                    break;

                var name = Path.GetFileNameWithoutExtension(assembly.Assembly);

                AppDomain domain = AppDomain.CreateDomain(name + "Domain", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);

                var scannerType = typeof (TestAssembly);
                _testAssembly = (TestAssembly) domain.CreateInstanceAndUnwrap(scannerType.Assembly.FullName, scannerType.FullName);
                var testResults = _testAssembly.RunTests(assembly.Assembly,
                                                         assembly.Parallel,
                                                         assembly.DegreeOfParallelism,
                                                         configuration.TestAssembly,
                                                         configuration.ResultsFile);

                assemblyResults.Add(assembly.Assembly, testResults);

                AppDomain.Unload(domain);
            }

            // These are only nice to have for the UI Application. Unecessary for the Console app.
            GC.Collect();
            GC.WaitForPendingFinalizers();

            stopWatch.Stop();
            DateTime end = DateTime.UtcNow;

            ITestFactory factory = TestFactoryLoader.Load(configuration.TestAssembly);
            var testRunner = factory.CreateTestRunner();

            List<TestResult> flattendResults = assemblyResults.Values.SelectMany(x => x).ToList();
            if (configuration.ResultsFile != null)
            {
                testRunner.WriteResults(start, end, flattendResults, assemblyResults, configuration.ResultsFile);
            }
            
            var failures = flattendResults.Where(c => !c.TestPassed).ToList();
            return new TestRunResult(stopWatch.ElapsedMilliseconds, failures, flattendResults.Count);
        }

        public Task Start(DateTime startTime, RuntimeConfiguration configuration)
        {
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            Task t = Task.Factory.StartNew(() =>
                                               {
                                                   IList<ISyncedDestination> synced = FolderSync.Sync(configuration.Destination, configuration.AssemblyList, token);
                                                   var infos = synced.SelectMany(x => x.AssembliesWithFullPath);
                                                   var result = LoadAssembliesAndRunTests(infos, token, configuration);

                                                   if (Finished != null)
                                                   {
                                                       var args = new TestRunnerFinishedEventArgs
                                                                      {
                                                                          StartTime = startTime,
                                                                          ElapsedDisplay = result.ElapsedDisplay,
                                                                          Failures = result.Failures,
                                                                          TestsRan = result.TestsRan
                                                                      };
                                                       Finished(this, args);
                                                   }
                                               },
                                           _tokenSource.Token);

            return t;
        }
    }
}