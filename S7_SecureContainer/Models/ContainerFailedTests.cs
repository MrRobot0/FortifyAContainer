using Docker.DotNet.Models;

namespace S7_SecureContainer.Models
{
    public class ContainerFailedTests
    {
        public List<ContainerListResponse> Containers = new();
        public int RetryCount { get; set; } = 0;

        public bool TestComplete { get; set; } = true;
    }
}
