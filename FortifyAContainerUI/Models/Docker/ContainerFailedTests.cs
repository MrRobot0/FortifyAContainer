using Docker.DotNet.Models;

namespace FortifyAContainerUI.Models.Docker
{
    public class ContainerFailedTests
    {
        public Dictionary<ContainerListResponse, List<ContainerTestType>> ContainerTestResults = new();
        public int RetryCount { get; set; } = 0;

        public bool TestComplete { get; set; } = true;
    }
}
