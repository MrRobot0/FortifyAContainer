namespace S7_SecureContainer.Models
{
    public class TestStillRunningExpection : Exception
    {
        public TestStillRunningExpection() : base("Test(s) are still running")
        {

        }
    }
}
