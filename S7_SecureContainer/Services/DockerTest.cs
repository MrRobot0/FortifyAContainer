using Docker.DotNet.Models;
using S7_SecureContainer.Models;

namespace S7_SecureContainer.Services
{
    public class DockerTest
    {
        private DockerService? DockerService { get; set; }

        private Dictionary<ContainerListResponse, List<TestResult>> containerTestResults = new Dictionary<ContainerListResponse, List<TestResult>>();

        private List<Task> Tasks= new List<Task>();

        public async Task<TestContainerModel> TestContainers(ContainersListParameters listParameters, DockerService dockerService)
        {
            TestContainerModel testContainerModel = new();
            if (!Task.WhenAll(Tasks).IsCompleted)
            {
                throw new TestStillRunningExpection();
            }

            this.DockerService = dockerService;
            var containers = await dockerService.Client.Containers.ListContainersAsync(listParameters);
            
            containerTestResults.Clear();
            Tasks.Clear();

            RunTests(containers);

            await Task.WhenAll(Tasks);
            ContainerFailedTests containerFailedTests = new();
            CheckIfTestAreComplete(containerTestResults, containerFailedTests);

            while (!containerFailedTests.TestComplete && containerFailedTests.RetryCount < 6)
            {
                RunTests(containerFailedTests.Containers);
                CheckIfTestAreComplete(containerTestResults, containerFailedTests);
                await Task.WhenAll(Tasks);
            }

            if (!containerFailedTests.TestComplete)
            {
                testContainerModel.Toasts.Add(TestToastsMessages.TestNotFullyComplete);
            }
            else
            {
                testContainerModel.Toasts.Add(TestToastsMessages.TestComplete);
            }

            testContainerModel.ContainerTestResults = containerTestResults;
            return testContainerModel;
        }

        private void RunTests(IList<ContainerListResponse> containers)
        {
            foreach (ContainerListResponse container in containers)
            {
                containerTestResults.Remove(container);
                containerTestResults.Add(container, new());
                Tasks.Add(Task.Run(() => CheckForRoot(container)));
                Tasks.Add(Task.Run(() => CheckForDefaultNetwork(container)));
            }
        }

        private void CheckIfTestAreComplete(Dictionary<ContainerListResponse, List<TestResult>> containerTestResults, 
            ContainerFailedTests containerFailedTests)
        {
            containerFailedTests.TestComplete = true;
            containerFailedTests.Containers.Clear();
            containerFailedTests.RetryCount++;
            foreach (var containerTestResult in containerTestResults)
            {
                if (containerTestResult.Value.Count == TestType.GetTestCount())
                {
                    containerFailedTests.TestComplete = false;
                    containerFailedTests.Containers.Add(containerTestResult.Key);
                }
            }
        }

        private async Task CheckForRoot(ContainerListResponse container)
        {
            var list = containerTestResults[container];
            try
            {
                var config = await DockerService.Client.Containers.InspectContainerAsync(container.ID);
                if (config.ID != container.ID)
                {
                    list.Add(new TestResult(TestType.CheckForRoot, TestResult.Status.Invalid, container));
                }
                var user = config.Config.User;
                if (user != "0:0" && user != "root")
                {
                    list.Add(new TestResult(TestType.CheckForRoot, TestResult.Status.Passed, container));
                }
                else
                {
                    list.Add(new TestResult(TestType.CheckForRoot, TestResult.Status.Failed, container));
                }
            }
            catch (Exception ex)
            {
                list.Add(new TestResult(TestType.CheckForRoot, TestResult.Status.Invalid, container));
            }
           
		}

        private async Task CheckForDefaultNetwork(ContainerListResponse container)
        {
            var list = containerTestResults[container];
            try
            {
                var config = await DockerService.Client.Containers.InspectContainerAsync(container.ID);
                if (config.ID != container.ID)
                {
                    list.Add(new TestResult(TestType.CheckForRoot, TestResult.Status.Invalid, container));
                }
                var networks = config.NetworkSettings.Networks;
                if (networks == null)
                {
                    list.Add(new TestResult(TestType.CheckForDefaultNetwork, TestResult.Status.Invalid, container));
                    return;
                }

                foreach (var network in networks)
                {
                    if (network.Key == "bridge")
                    {
                        list.Add(new TestResult(TestType.CheckForDefaultNetwork, TestResult.Status.Failed, container));
                        return;
                    }
                }
                list.Add(new TestResult(TestType.CheckForDefaultNetwork, TestResult.Status.Passed, container));
            }
            catch (Exception ex)
            {
                list.Add(new TestResult(TestType.CheckForDefaultNetwork, TestResult.Status.Invalid, container));
            }
        }
    }
}
