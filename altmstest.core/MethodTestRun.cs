using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AltMstest.Core
{
    public class MethodTestRun
    {
        public MethodInfo Method { get; set; }
        public ExpectedExceptionAttribute ExpectedException { get; set; }
    }
}