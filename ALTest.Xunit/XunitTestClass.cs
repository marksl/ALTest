using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ALTest.Core;
using Xunit;
using Xunit.Sdk;
using TestResult = ALTest.Core.TestResult;

namespace ALTest.Xunit
{

    public class XunitTestMethod
    {
        public MethodInfo Method { get; set; }
        public FactAttribute Fact { get; set; }
    }

    public class XunitTestClass : TestClass
    {
        public List<XunitTestMethod> XunitMethods { get; set; }
        public RunWithAttribute RunWith { get; set; }

        private readonly Type _classType;
        public XunitTestClass(Type classType)
            : base(classType)
        {
            fixtures = new Dictionary<Type, Tuple<MethodInfo, object>>();
            _classType = classType;

            XunitMethods = new List<XunitTestMethod>();
        }

        public void AddFixture(Type fixtureType)
        {
            fixtures.Add(fixtureType, null);
            fff.Add(fixtureType);
        }


        // Invoke SetFixture for each of the IUseFixture<> interfaces.
        // This is done for each test intialize
        public void InvokeTestInitialize(object instance, string testName)
        {
            foreach (var f in fixtures)
            {
                f.Value.Item1.Invoke(instance, new[] {f.Value.Item2});
            }
        }

        public override List<TestResult> Run(System.Threading.CancellationToken ct, ITestRunner testRunner)
        {
            var results = new List<TestResult>(100);

            if (ct.IsCancellationRequested)
                return results;

            Exception allException = null;

            DateTime start = DateTime.Now;
            var watch = Stopwatch.StartNew();


            // Total hack to handle custom ClassCommands.
            ITestClassCommand classCommand = null;

            // Dear god. this frankenstein code works.. blech.
            if (this.RunWith != null)
            {
                classCommand = (ITestClassCommand) Activator.CreateInstance(RunWith.TestClassCommand);
                classCommand.TypeUnderTest = Reflector.Wrap(_classType);

                Predicate<ITestResult> resultCallback =
                    (x) =>
                        {
                            var sucess = x as PassedResult;
                            if (sucess != null)
                            {
                                AddResult(results, sucess.MethodName, allException, start, watch);
                            }
                            var fail = x as FailedResult;
                            if (fail != null)
                            {
                                AddResult(results, fail.MethodName, fail.Output, fail.Message, fail.StackTrace, start, watch);
                            }

                            return true;
                        };
                TestClassCommandRunner.Execute(classCommand, null, null, resultCallback);
                return results;
            }

            // Class Initialize
            try
            {
                RunClassInitialize();
            }
            catch (Exception e)
            {
                allException = e;
            }



            if (allException != null)
            {
                foreach (var testMethod in TestMethods)
                {
                    AddResult(results, testMethod, allException, start, watch);
                }

                return results;
            }

            foreach (var testMethod in this.XunitMethods)
            {
                if (ct.IsCancellationRequested)
                    return new List<TestResult>();

                foreach (ITestCommand command in  GetCommands(classCommand, testMethod))
                {
                    try
                    {
                        IMethodInfo method = Reflector.Wrap(testMethod.Method);
                        ITestCommand wrappedCommand = new BeforeAfterCommand(command, testMethod.Method);

                        if (command.ShouldCreateInstance)
                            wrappedCommand = new LifetimeCommand(wrappedCommand,  method);

                        wrappedCommand = new ExceptionAndOutputCaptureCommand(wrappedCommand, method);
                        wrappedCommand = new TimedCommand(wrappedCommand);

                        if (wrappedCommand.Timeout > 0)
                            wrappedCommand = new TimeoutCommand(wrappedCommand, wrappedCommand.Timeout, method);



                        object instance = Activator.CreateInstance(_classType);
                        InvokeTestInitialize(instance, testMethod.Method.Name);

                        
                        MethodResult result = wrappedCommand.Execute(instance);
                        if (result is PassedResult)
                        {
                            AddResult(results, new TestMethod {Method = testMethod.Method}, null, start, watch);
                        }
                    }
                    catch (Exception exception)
                    {
                        AddResult(results, new TestMethod {Method = testMethod.Method}, exception, start, watch);
                    }
                }

                //else if (result is FailedResult)
                //{
                //    var failedResult = (FailedResult) result;
                //    AddResult(results, new TestMethod { Method = testMethod.Method }, failedResult.Output, failedResult.Message, failedResult.StackTrace, start, watch);
                //}
                //else
                //{

                //}
            }

            try
            {
                RunClassCleanup();

                if (classCommand != null)
                    classCommand.ClassFinish();
            }
            catch { }

            return results;
        }

        private IEnumerable<ITestCommand> GetCommands(ITestClassCommand command, XunitTestMethod testMethod)
        {
            var methodInfo = Reflector.Wrap(testMethod.Method);

            if (command !=null)
            {
                return command.EnumerateTestCommands(methodInfo);
            }

            return testMethod.Fact.CreateTestCommands(methodInfo);
        }

        protected void AddResult(ICollection<TestResult> results, string testMethodName, string exceptionString,
            string exceptionMessage, string exceptionStacktrace,
            DateTime start, Stopwatch watch)
        {
            StdOut.Write("{0,-22}", exceptionString == null ? "Passed" : "Failed");
            StdOut.WriteLine("{0}.{1}", _classType.FullName, testMethodName);

            var testResult = new TestResult(testMethodName, exceptionString == null, _classType.FullName, exceptionString,
                exceptionMessage, exceptionStacktrace)
            {
                Duration = watch.Elapsed,
                StartTime = start,
                EndTime = DateTime.Now
            };
            results.Add(testResult);

            watch.Restart();
        }


        private readonly Dictionary<Type, Tuple<MethodInfo, object>> fixtures;

        protected override void RunClassInitialize()
        {
            Type[] keyCollection = fixtures.Keys.Select(x => x).ToArray();
            foreach (var t in keyCollection)
            {
                var instance = Activator.CreateInstance(t);
                var methodInfo = _classType.GetMethod("SetFixture", new[] {t});
                fixtures[t] = new Tuple<MethodInfo, object>(methodInfo, instance);
            }
        }

        protected override void RunClassCleanup()
        {
            object[] valueCollection = fixtures.Values.Select(x=>x.Item2).ToArray();
            foreach (var instance in valueCollection)
            {
                var disposable = instance as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }
    }
}