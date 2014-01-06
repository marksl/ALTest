using System;
using System.Collections.Generic;

namespace AltMstest.Core
{
    public class TestRunnerFinishedEventArgs : EventArgs
    {
        public DateTime StartTime { get; set; }
        public string ElapsedDisplay { get; set; }
        public IList<TestResult> Failures { get; set; }
    }
}