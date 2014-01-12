using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Experiment
{
    [TestClass]
    public class TestC
    {
        [ClassInitialize()]
        public static void ClassInitC(TestContext context)
        {

        }
        [TestInitialize()]
        public void InitializeC()
        {

        }

        [TestCleanup()]
        public void CleanupC()
        {

        }
    }

    [TestClass]
    public class TestB : TestC
    {
        [ClassInitialize()]
        public static void ClassInitB(TestContext context)
        {

        }
        [TestInitialize()]
        public void InitializeB()
        {

        }

        [TestCleanup()]
        public void CleanupB()
        {

        }
    }

    [TestClass]
    public class TestA : TestC
    {
        [ClassInitialize()]
        public static void ClassInitA(TestContext context)
        {

        }

        [TestMethod]
        public void TestA1()
        {
            
        }

    }

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
