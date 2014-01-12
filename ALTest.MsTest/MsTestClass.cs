using System;
using System.Reflection;
using ALTest.Core;

namespace ALTest.MsTest
{
    public class MsTestClass : TestClass
    {
        public MsTestClass(Type classType)
            : base(classType)
        {
        }

        public PropertyInfo TestContextMethod { get; set; }

        protected override void RunClassInitialize()
        {
            foreach (var classInit in ClassInitialize)
            {
                classInit.Invoke(null, new object[] {new MsTestContext()});
            }
        }
    }
}