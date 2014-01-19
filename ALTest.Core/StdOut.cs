using System;

namespace ALTest.Core
{
    public static class StdOut
    {
        // TODO: Buffer it. http://msdn.microsoft.com/en-us/library/system.io.textwriter.writelineasync(v=vs.110).aspx
        public static bool Silent { get; set; }
        
        public static void WriteLine()
        {
            if (!Silent)
            {
                Console.Out.WriteLine();
            }
        }

        public static void WriteLine(string format, params object[] args)
        {
            if (!Silent)
            {
                Console.WriteLine(format, args);
            }
        }

        public static void Write(string format, params object[] args)
        {
            if (!Silent)
            {
                Console.Write(format, args);
            }
        }
    }
}