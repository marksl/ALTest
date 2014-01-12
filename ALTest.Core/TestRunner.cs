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

        private TestRunResult LoadAssembliesAndRunTests(IEnumerable<AssemblyInfo> assemblies, CancellationToken ct, string testAssembly)
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

                var scannerType = typeof (TestAssembly);
                _testAssembly = (TestAssembly) domain.CreateInstanceAndUnwrap(scannerType.Assembly.FullName, scannerType.FullName);

                var testResults = _testAssembly.RunTests(assembly.Assembly, assembly.Parallel, assembly.DegreeOfParallelism, testAssembly);

                // Deep Copy
                failures.AddRange(testResults.Select(tr =>
                                                     new TestResult(
                                                         tr.TestName,
                                                         tr.TestPassed,
                                                         tr.ClassName,
                                                         tr.ExceptionString
                                                         )));

                AppDomain.Unload(domain);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            stopWatch.Stop();

            return new TestRunResult(stopWatch.ElapsedMilliseconds, failures);
        }

        public Task Start(DateTime startTime, string destination, IList<AssemblyConfigElement> assemblyList, string testAssembly)
        {
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            Task t = Task.Factory.StartNew(() =>
                                               {
                                                   IList<ISyncedDestination> synced = FolderSync.Sync(destination, assemblyList, token);
                                                   var infos = synced.SelectMany(x => x.AssembliesWithFullPath);
                                                   var result = LoadAssembliesAndRunTests(infos, token, testAssembly);

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