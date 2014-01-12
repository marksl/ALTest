using System;
using System.Collections.Generic;

namespace ALTest.Core
{
    public class TestRunResult
    {
        public TestRunResult(long elapsedMilliseconds, List<TestResult> failures, int testsRan)
        {
            ElapsedSeconds = (int) (elapsedMilliseconds/1000);
            Failures = failures;
            TestsRan = testsRan;
        }

        public int ElapsedSeconds { get; private set; }
        public IList<TestResult> Failures { get; private set; }
        public int TestsRan { get; private set; }

        public string ElapsedDisplay
        {
            get
            {
                int secs = ElapsedSeconds%60;
                int totalMins = (ElapsedSeconds - secs)/60;
                int mins = totalMins%60;

                int totalHours = (totalMins - mins)/60;
                int hours = totalHours%60;

                int days = (totalHours - hours)/24;

                String s = "";
                if (days != 0)
                {
                    s += days + ":";
                }

                if (hours != 0)
                {
                    s += hours.ToString("D2") + ":";
                }

                s += mins.ToString("D2") + ":";
                s += secs.ToString("D2");

                return s;
            }
        }
    }
}