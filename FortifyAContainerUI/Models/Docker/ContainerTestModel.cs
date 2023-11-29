using Docker.DotNet.Models;
using FortifyAContainerUI.Models.Test;

namespace FortifyAContainerUI.Models.Docker
{
    public class ContainerTestModel
    {
        public Dictionary<ContainerListResponse, List<TestResult>> Results { get; set; } = new();
    }
}
