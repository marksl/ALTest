using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AltMstest.Core.Configuration;

namespace AltMstest.Core
{
    public class TestRunnerLauncher
    {
        public EventHandler<TestRunnerFinishedEventArgs> Finished;

        private TestRunner _testRunner;

        private CancellationTokenSource _tokenSource;

        public void Cancel()
        {
            _tokenSource.Cancel();

            if (_testRunner != null)
            {
                _testRunner.Cancel();
            }
        }

        private TestRunResult LoadAssembliesAndRunTests(IEnumerable<AssemblyInfo> assemblies, CancellationToken ct)
        {
            var failures=  new List<TestResult>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var assembly in assemblies)
            {
                if (ct.IsCancellationRequested)
                    break;

                var name = Path.GetFileNameWithoutExtension(assembly.Assembly);

                AppDomain domain = AppDomain.CreateDomain(name + "Domain", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);

                var scannerType = typeof (TestRunner);
                _testRunner = (TestRunner) domain.CreateInstanceAndUnwrap(scannerType.Assembly.FullName, scannerType.FullName);

                var testResults = _testRunner.RunTests(assembly.Assembly, assembly.Parallel);

                // Deep Copy
                failures.AddRange(testResults.Select(tr =>
                                                     new TestResult(
                                                         tr.TestName,
                                                         tr.TestPassed,
                                                         tr.ClassName,
                                                         tr.StackTrace
                                                         )));

                AppDomain.Unload(domain);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            stopWatch.Stop();

            return new TestRunResult(stopWatch.ElapsedMilliseconds, failures);
        }

        public Task Start(DateTime startTime, string destination, IList<AssemblyConfigElement> assemblyList)
        {
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            Task t = Task.Factory.StartNew(() =>
                                               {
                                                   IList<ISyncedDestination> synced = FolderSync.Sync(destination, assemblyList, token);
                                                   var infos = synced.SelectMany(x => x.AssembliesWithFullPath);
                                                   var result = LoadAssembliesAndRunTests(infos, token);

                                                   if (Finished != null)
                                                   {
                                                       var args = new TestRunnerFinishedEventArgs
                                                                      {
                                                                          StartTime = startTime,
                                                                          ElapsedDisplay = result.ElapsedDisplay,
                                                                          Failures = result.Failures
                                                                      };
                                                       Finished(this, args);
                                                   }
                                               },
                                           _tokenSource.Token);

            return t;
        }
    }
}