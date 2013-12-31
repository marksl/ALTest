using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Experiment
{
    [TestClass]
    public class UnitTest1
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
         
        }

        [TestInitialize()]
        public void Initialize()
        {
         
        }

        [TestCleanup()]
        public void Cleanup()
        {
         
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
         
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
         
        }

        [TestMethod()]
        public void AlwaysPasses()
        {

        }

        [TestMethod()]
        [ExpectedException(typeof(System.DivideByZeroException))]
        public void ShouldThrowException()
        {
         
        }

    }
}
