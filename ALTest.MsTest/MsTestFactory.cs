using ALTest.Core;

namespace ALTest.MsTest
{
    public class MsTestFactory : ITestFactory
    {
        public ITestLoader CreateTestLoader()
        {
            return new MsTestLoader();
        }

        public ITestRunner CreateTestRunner()
        {
            return new MsTestRunner();
        }
    }
}
