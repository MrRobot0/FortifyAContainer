using Docker.DotNet.Models;
using S7_SecureContainer.Interfaces.Container;
using S7_SecureContainer.Models.Test;
using S7_SecureContainer.Services;

namespace S7_SecureContainer.Models.Docker
{
    public class ContainerTests : ITests<ContainerListResponse>
    {
        private readonly DockerService DockerService;
        private readonly Dictionary<ContainerListResponse, List<TestResult>> containerTestResults;

        public ContainerTests(DockerService dockerService, Dictionary<ContainerListResponse, List<TestResult>> containerTestResults)
        {
            DockerService = dockerService;
            this.containerTestResults = containerTestResults;
        }
        private ContainerTests() { }

        public async Task<List<ContainerListResponse>> getContainerList(ContainersListParameters listParameters)
        {
            IList<ContainerListResponse> containers = await DockerService.Client.Containers.ListContainersAsync(listParameters);
            return containers.ToList();
        }

        public async Task CheckForRoot(ContainerListResponse container)
        {
            var list = containerTestResults[container];
            try
            {
                var config = await DockerService.Client.Containers.InspectContainerAsync(container.ID);
                if (config.ID != container.ID)
                {
                    lock (containerTestResults)
                        list.Add(new TestResult(ContainerTestTypes.CheckForRoot, TestResult.Status.Invalid, container));
                }
                var user = config.Config.User;
                if (user != "0:0" && user != "root")
                {
                    lock (containerTestResults)
                        list.Add(new TestResult(ContainerTestTypes.CheckForRoot, TestResult.Status.Passed, container));
                }
                else
                {
                    lock (containerTestResults)
                        list.Add(new TestResult(ContainerTestTypes.CheckForRoot, TestResult.Status.Failed, container));
                }
            }
            catch (Exception)
            {
                lock (containerTestResults)
                    list.Add(new TestResult(ContainerTestTypes.CheckForRoot, TestResult.Status.Invalid, container));
            }
        }

        public async Task CheckForDefaultNetwork(ContainerListResponse container)
        {
            var list = containerTestResults[container];
            try
            {   
                var config = await DockerService.Client.Containers.InspectContainerAsync(container.ID);
                if (config.ID != container.ID)
                {
                    lock (containerTestResults)
                        list.Add(new TestResult(ContainerTestTypes.CheckForRoot, TestResult.Status.Invalid, container));
                }
                var networks = config.NetworkSettings.Networks;
                if (networks == null)
                {
                    lock (containerTestResults)
                        list.Add(new TestResult(ContainerTestTypes.CheckForDefaultNetwork, TestResult.Status.Invalid, container));
                    return;
                }

                foreach (var network in networks)
                {
                    if (network.Key == "bridge")
                    {
                        lock (containerTestResults)
                            list.Add(new TestResult(ContainerTestTypes.CheckForDefaultNetwork, TestResult.Status.Failed, container));
                        return;
                    }
                }
                lock (containerTestResults)
                    list.Add(new TestResult(ContainerTestTypes.CheckForDefaultNetwork, TestResult.Status.Passed, container));
            }
            catch (Exception)
            {
                lock (containerTestResults)
                    list.Add(new TestResult(ContainerTestTypes.CheckForDefaultNetwork, TestResult.Status.Invalid, container));
            }
        }
    }
}
