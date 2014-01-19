using System;

namespace ALTest.Core
{
    [Serializable]
    public class TestResult
    {
        public TestResult(string testName, bool testPassed,
                          string className, string exceptionString)
            : this()
        {
            TestName = testName;
            TestPassed = testPassed;
            ClassName = className;
            ExceptionString = exceptionString;
        }

        public TestResult()
        {
        }

        public string ClassName { get; set; }
        public string TestName { get; set; }
        public bool TestPassed { get; set; }
        public string ExceptionString { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", TestName, TestPassed);
        }
    }
}