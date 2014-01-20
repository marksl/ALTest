using System;
using System.IO;

namespace ALTest.Core
{
    public static class StdOut
    {
        public static bool Silent { get; set; }

        public static void WriteLine()
        {
            if (!Silent)
            {
                standardOutput.WriteLine();
            }
        }

        static private TextWriter standardOutput;

        public static void Init()
        {
            standardOutput = new StreamWriter(Console.OpenStandardOutput(8000));
            standardOutput = TextWriter.Synchronized(standardOutput);
        }

        public static void Flush()
        {
            standardOutput.Flush();
        }

        public static void WriteLine(string format, params object[] args)
        {
            if (!Silent)
            {
                standardOutput.WriteLine(format, args);
            }
        }

        public static void Write(string format, params object[] args)
        {
            if (!Silent)
            {
                standardOutput.Write(format, args);
            }
        }
    }
}