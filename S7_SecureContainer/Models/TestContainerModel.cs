using Docker.DotNet.Models;

namespace S7_SecureContainer.Models
{
    public class TestContainerModel
    {
        public Dictionary<ContainerListResponse, List<TestResult>> ContainerTestResults { get; set; } = new();
        public List<ToastModel> Toasts { get; set; } = new();
    }
}
