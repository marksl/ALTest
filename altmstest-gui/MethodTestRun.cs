using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AltMstestGui
{
    public class MethodTestRun
    {
        public ExpectedExceptionAttribute ExpectedException { get; set; }
        public MethodInfo Method { get; set; }
    }
}