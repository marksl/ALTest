using System;

namespace ALTest.Core
{
    [Serializable]
    public class TestResult
    {
        public TestResult(string testName, bool testPassed,
                          string className, Exception ex)
            : this()
        {
            TestName = testName;
            TestPassed = testPassed;
            ClassName = className;
            if (ex != null)
            {
                ExceptionString = ex.ToString();
                ExceptionMessage = ex.Message;
                ExceptionStackTrace = ex.StackTrace;
            }
        }

        public TestResult(string testName, bool testPassed,
                          string className, string exceptionString, 
                            string exceptionMessage, string exceptionStackTrace)
            :this(testName, testPassed, className,null)
        {
            ExceptionString = exceptionString;
            ExceptionMessage = exceptionMessage;
            ExceptionStackTrace = exceptionStackTrace;
        }

        public TestResult()
        {
        }

        public string ClassName { get; set; }
        public string TestName { get; set; }
        public bool TestPassed { get; set; }
        public string ExceptionString { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", TestName, TestPassed);
        }
    }
}