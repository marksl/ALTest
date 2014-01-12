using System;
using System.Reflection;

namespace ALTest.Core
{
    public class TestMethod
    {
        public MethodInfo Method { get; set; }
        public Type ExpectedExceptionType { get; set; }
    }
}