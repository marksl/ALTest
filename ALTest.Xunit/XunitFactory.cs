using ALTest.Core;

namespace ALTest.Xunit
{
    public class XunitFactory : ITestFactory
    {
        public ITestLoader CreateTestLoader()
        {
            return new XunitTestLoader();
        }

        public ITestRunner CreateTestRunner()
        {
            return new XunitTestRunner();
        }
    }
}