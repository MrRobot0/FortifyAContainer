using System.ComponentModel;

namespace S7_SecureContainer.Interfaces.Test
{
    public interface ITestRunner<T, Q>
    {
        public abstract void RunTestsOnContainer(List<string> tests, T container);
        public abstract static void RunTests(List<T> containers);
        public abstract static void ReRunTests(Q containerFailedTests);
    }
}
