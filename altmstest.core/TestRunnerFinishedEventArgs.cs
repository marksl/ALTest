using System;

namespace AltMstest.Core
{
    public class TestRunnerFinishedEventArgs : EventArgs
    {
        public DateTime StartTime { get; set; }
        public string ElapsedDisplay { get; set; }
    }
}