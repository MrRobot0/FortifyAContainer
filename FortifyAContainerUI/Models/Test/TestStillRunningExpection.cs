namespace FortifyAContainerUI.Models.Test
{
    public class TestStillRunningExpection : Exception
    {
        public TestStillRunningExpection() : base("Test(s) are still running")
        {

        }
    }
}
