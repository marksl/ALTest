using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Experiment
{
    public class MyFixture : IDisposable
    {
        // Class initialize
        public MyFixture()
        {
            
        }
        // Class cleanup
        public void Dispose()
        {
            
        }
    }

    public class MyTestClass : IUseFixture<MyFixture>
    {
        // Call this method for every testinitalize!
        public void SetFixture(MyFixture data)
        {
            
        }
    }

    [TestClass]
    public class XunitMuck
    {
        [TestMethod]
        public void MethodUnderTest_Scenario_ExpectedBehaviour()
        {
            var results = typeof (MyTestClass).GetInterfaces()
                                              .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof (IUseFixture<>)).ToList();


            var results2 = results[0].GetGenericArguments();
            //results[0].GenericTypeArguments[0]
            Assert.IsNotNull(results);
        }
    }
}