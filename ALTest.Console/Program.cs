using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ALTest.Core;
using ALTest.Core.Configuration;

namespace ALTest.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StdOut.Init();

            var versionAttribute =
                (AssemblyFileVersionAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyFileVersionAttribute), false)[0];
            StdOut.WriteLine("ALTest Version {0}", versionAttribute.Version);

            // MsTest
            const string testcontainerPrefix = "/testcontainer:";

            // ALTest
            const string workingfolderPrefix = "/workingfolder:";
            // - If not specified then create /altest-temp/ subfolder in current path.
            const string parallelPrefix = "/parallel:";
            // - default parallel setting for all test containers. makes all assemblies run in parallel or not...
            // - Defaults to false <- backwards compatability with MsTest
            const string degreeOfParallelismPrefix = "/degreeofparallelism:";
            // - optional. Controls how PLinq manages the Threading..
            const string testRunnerPrefix = "/testrunner:";
            // - optional. Default is ALTest.MsTest.dll

            const string resultsFilePrefix = "/resultsfile:";

            string workingFolder = args.SingleOrDefault(a => a.StartsWith(workingfolderPrefix, StringComparison.InvariantCultureIgnoreCase));
            workingFolder = workingFolder == null
                                ? Path.Combine(Directory.GetCurrentDirectory(), "ALTestWorkingFolder")
                                : workingFolder.Substring(workingfolderPrefix.Length, workingFolder.Length - workingfolderPrefix.Length);

            bool parallel = false;
            string parallelString = args.SingleOrDefault(a => a.StartsWith(parallelPrefix, StringComparison.InvariantCultureIgnoreCase));
            if (parallelString != null)
            {
                parallelString = parallelString.Substring(parallelPrefix.Length, parallelString.Length - parallelPrefix.Length);
                parallel = parallelString.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }

            int? degreeOfParallelism = null;
            string degreeOfParallelismString = args.SingleOrDefault(a => a.StartsWith(degreeOfParallelismPrefix, StringComparison.InvariantCultureIgnoreCase));
            if (degreeOfParallelismString != null)
            {
                degreeOfParallelismString = degreeOfParallelismString.Substring(degreeOfParallelismPrefix.Length,
                                                                                degreeOfParallelismString.Length - degreeOfParallelismPrefix.Length);

                int parsedInt;
                if (int.TryParse(degreeOfParallelismString, out parsedInt))
                {
                    degreeOfParallelism = parsedInt;
                }
            }

            IEnumerable<string> testContainers = args.Where(a => a.StartsWith(testcontainerPrefix, StringComparison.InvariantCultureIgnoreCase))
                                                     .Select(tc => tc.Substring(testcontainerPrefix.Length, tc.Length - testcontainerPrefix.Length));

            var assemblyList = new List<AssemblyConfigElement>();

            foreach (var taa in testContainers)
            {
                Uri uri;
                if (Uri.TryCreate(taa, UriKind.RelativeOrAbsolute, out uri))
                {
                    string fullPath = uri.IsAbsoluteUri ? taa : Path.Combine(Directory.GetCurrentDirectory(), taa);

                    assemblyList.Add(new AssemblyConfigElement
                                         {
                                             Assembly = fullPath,
                                             RunParallel = parallel,
                                             DegreeOfParallelism = degreeOfParallelism
                                         });
                }
            }

            string testRunner = args.SingleOrDefault(a => a.StartsWith(testRunnerPrefix, StringComparison.InvariantCultureIgnoreCase));
            testRunner = testRunner == null
                             ? Path.Combine(Directory.GetCurrentDirectory(), "ALTest.MsTest.dll")
                             : testRunner.Substring(testRunnerPrefix.Length, testRunner.Length - testRunnerPrefix.Length);

            string resultsFile = args.SingleOrDefault(a => a.StartsWith(resultsFilePrefix, StringComparison.InvariantCultureIgnoreCase));
            if (resultsFile != null)
            {
                resultsFile = resultsFile.Substring(resultsFilePrefix.Length, resultsFile.Length - resultsFilePrefix.Length);
            }

            // MStest - Initially only need to support 
            // TODO:  /detail: !!! stacktrace, etc

            // ALTest
            // TODO:  /testcontainer-parallel-strategy-5:Footests.dll

            var configuration = new RuntimeConfiguration(workingFolder, assemblyList, testRunner, false, resultsFile);
            var runner = new TestRunner();
            runner.Finished += Finished;
            var task = runner.Start(DateTime.Now, configuration);

            task.Wait();

            StdOut.Flush();
        }

        private static void Finished(object sender, TestRunnerFinishedEventArgs e)
        {
            var failuresCount = e.Failures.Count;
            var successCount = e.TestsRan - failuresCount;

            StdOut.Write("{0}/{1} test(s) Passed",
                         successCount,
                         e.TestsRan);

            if (failuresCount > 0)
            {
                StdOut.WriteLine(", {0} Failed", failuresCount);
            }
            else
            {
                StdOut.WriteLine();
            }

            StdOut.WriteLine();
            StdOut.WriteLine("Summary");
            StdOut.WriteLine("-------");
            StdOut.WriteLine(failuresCount > 0 ? "Test Run Failed." : "Test Run Completed.");


            StdOut.WriteLine("  Passed{0,8}", successCount);
            if (failuresCount > 0)
                StdOut.WriteLine("  Failed{0,8}", failuresCount);
            StdOut.WriteLine("  {0,14}", new string('-', 14));
            StdOut.WriteLine("  Total {0,8}", successCount + failuresCount);
        }
    }
}