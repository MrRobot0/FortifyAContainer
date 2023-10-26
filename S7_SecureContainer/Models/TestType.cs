namespace S7_SecureContainer.Models
{
    public static class TestType
    {
        public const string CheckForRoot = "Check for root usage";
        public const string CheckForDefaultNetwork = "Check for default network";

        public static List<string> All = new()
        {
            { CheckForRoot },
            { CheckForDefaultNetwork },
        };
        public static int GetTestCount() { return All.Count; }
    }
}
