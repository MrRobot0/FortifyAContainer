namespace S7_SecureContainer.Models.Docker
{
    public static class ContainerTestTypes
    {
        public const string CheckForRoot = "Root user";
        public const string CheckForDefaultNetwork = "Bridge network";

        public static List<string> All = new()
        {
            { CheckForRoot },
            { CheckForDefaultNetwork },
        };
        public static int GetTestCount() { return All.Count; }
    }
}
