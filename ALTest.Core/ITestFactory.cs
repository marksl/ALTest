namespace ALTest.Core
{
    public interface ITestFactory
    {
        ITestLoader CreateTestLoader();
        ITestRunner CreateTestRunner();
    }
}