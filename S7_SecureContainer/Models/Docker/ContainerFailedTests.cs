using Docker.DotNet.Models;

namespace S7_SecureContainer.Models.Docker
{
    public class ContainerFailedTests
    {
        public Dictionary<ContainerListResponse, List<string>> ContainerTestResults = new();
        public int RetryCount { get; set; } = 0;

        public bool TestComplete { get; set; } = true;
    }
}
