using System;
using System.Collections.Generic;

namespace ALTest.Core
{
    public class TestRunnerFinishedEventArgs : EventArgs
    {
        public DateTime StartTime { get; set; }
        public string ElapsedDisplay { get; set; }
        public IList<TestResult> Failures { get; set; }
    }
}