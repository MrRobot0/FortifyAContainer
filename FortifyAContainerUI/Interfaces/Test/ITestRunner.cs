using System.ComponentModel;

namespace FortifyAContainerUI.Interfaces.Test
{
    public interface ITestRunner<T, Q>
    {
        public abstract void RunTestsOnContainer(List<string> tests, T container);
        public abstract static void RunTests(List<T> containers);
        public abstract static void ReRunTests(Q containerFailedTests);
    }
}
