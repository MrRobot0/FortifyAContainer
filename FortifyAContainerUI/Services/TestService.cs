using Blazored.Toast.Services;
using Docker.DotNet.Models;
using FortifyAContainerUI.Models.Docker;
using FortifyAContainerUI.Models.Test;
using FortifyAContainerUI.Models.Tests;

namespace FortifyAContainerUI.Services
{
    public class TestService
    {
        private readonly List<Task> Tasks= new();
        private readonly Dictionary<ContainerListResponse, List<TestResult>> containerTestResults = new();
        private readonly DockerService DockerService;
        private readonly IToastService ToastService;
        private readonly TestToastsMessages testToastsMessages;

        public bool TestRunning { get; private set; } = false;

        public TestService (IToastService toastService, DockerService dockerService) {
            ToastService = toastService;
            DockerService = dockerService;
            testToastsMessages = new TestToastsMessages(toastService);
        }

        public async Task<ContainerTestModel> TestDockerContainers()
        {
            if (!Task.WhenAll(Tasks).IsCompleted)
            {
                throw new TestStillRunningExpection();
            }
            if (DockerService.ConnectionString == string.Empty)
            {
                ToastService.ShowToast(ToastLevel.Warning, "Not connected!");
                return new();
            }
            TestRunning = true;
            testToastsMessages.stopwatch.Restart();
            ToastService.ShowToast(ToastLevel.Info, "Running tests..");

            ContainerFailedTests containerFailedTests = new();
            ContainerTestModel testContainerModel = new();
            ContainersListParameters listParameters = new()
            {
                All = true
            };
            List<ContainerListResponse> containers = await GetContainerList(listParameters);

            containerTestResults.Clear();
            Tasks.Clear();

            RunTests(containers);

            await Task.WhenAll(Tasks);
            CheckIfTestsAreComplete(containerTestResults, containerFailedTests);

            while (!containerFailedTests.TestComplete 
                && containerFailedTests.RetryCount <= TestToastsMessages.MaxRetries)
            {
                Tasks.Clear();
                ReRunTests(containerFailedTests);
                await Task.WhenAll(Tasks);
                CheckIfTestsAreComplete(containerTestResults, containerFailedTests);
            }

            testContainerModel.Results = containerTestResults;
            TestRunning = false;
            testToastsMessages.stopwatch.Stop();

            if (containerFailedTests.TestComplete)
                testToastsMessages.ShowToast(testToastsMessages.TestComplete());
            else
                testToastsMessages.ShowToast(testToastsMessages.TestNotFullyComplete());

            return testContainerModel;
        }

        private async Task<List<ContainerListResponse>> GetContainerList(ContainersListParameters listParameters)
        {
            if (DockerService.Client == null ) return new();
            IList<ContainerListResponse> containers = await DockerService.Client.Containers.ListContainersAsync(listParameters);
            return containers.ToList();
        }

        private void RunTests(List<ContainerListResponse> containers)
        {
            foreach (ContainerListResponse container in containers)
            {
                CleanContainerTestResult(container);
                RunTestsOnContainer(ContainerTestTypes.All, container);
            }
        }

        private void ReRunTests(ContainerFailedTests containerFailedTests)
        {
            foreach (var keyValuePair in containerFailedTests.ContainerTestResults)
            {
                var container = keyValuePair.Key;
                var failedTests = keyValuePair.Value;
                if (failedTests.Count == ContainerTestTypes.GetTestCount())
                {
                    CleanContainerTestResult(container);
                }
                RunTestsOnContainer(failedTests, container);
            }
        }

        private void RunTestsOnContainer(List<ContainerTestType> tests, ContainerListResponse container)
        {
            if (DockerService.Client == null) throw new Exception("DockerService is not available");
            Task<ContainerInspectResponse> containerInspectResponse = DockerService.Client.Containers.InspectContainerAsync(container.ID);
            foreach (var testType in tests)
            {
                switch (testType.Name)
                {
                    case ContainerTestTypes.Root:
                        lock (Tasks)
                            Tasks.Add(Task.Run(async () => containerTestResults[container].Add(await CheckForRoot.Run(await containerInspectResponse))));
                        break;
                    case ContainerTestTypes.DefaultNetwork:
                        lock (Tasks)
                            Tasks.Add(Task.Run(async () => containerTestResults[container].Add(await CheckForDefaultNetwork.Run(await containerInspectResponse))));
                        break;
                    case ContainerTestTypes.DockerSocket:
                        lock (Tasks)
                            Tasks.Add(Task.Run(async () => containerTestResults[container].Add(await CheckDockerSocket.Run(await containerInspectResponse))));
                        break;
                    case ContainerTestTypes.CPULimit:
                        lock (Tasks)
                            Tasks.Add(Task.Run(async () => containerTestResults[container].Add(await CheckCPULimit.Run(await containerInspectResponse))));
                        break;
					case ContainerTestTypes.MemLimit:
						lock (Tasks)
							Tasks.Add(Task.Run(async () => containerTestResults[container].Add(await CheckMemLimit.Run(await containerInspectResponse))));
						break;
					default:
                        throw new Exception("Test does not exist!");
                }
            }
        }

        private void CleanContainerTestResult(ContainerListResponse container)
        {
            containerTestResults.Remove(container);
            containerTestResults.Add(container, new());
        }

        private void CheckIfTestsAreComplete(Dictionary<ContainerListResponse, List<TestResult>> containerTestResults, 
            ContainerFailedTests containerFailedTests)
        {
            bool testComplete = true;
            containerFailedTests.ContainerTestResults.Clear();
            containerFailedTests.RetryCount++;
            foreach (var containerTestResult in containerTestResults)
            {
                if (containerTestResult.Value.Count != ContainerTestTypes.GetTestCount())
                {
                    testComplete = false;
                    containerFailedTests.ContainerTestResults.Add(
                        containerTestResult.Key, 
                        GetMissingTestResults(containerTestResult.Value));
                }
            }
            containerFailedTests.TestComplete = testComplete;
        }

        private List<ContainerTestType> GetMissingTestResults(List<TestResult> testResults)
        {
            List<ContainerTestType> testTypes = new();
            foreach (var testResult in ContainerTestTypes.All){
                if (!testResults.Any(a => a.Message == testResult.Name))
                {
                    testTypes.Add(ContainerTestTypes.All.Where(a => a.Name == testResult.Name).Single());
                }
            }
            return testTypes;
        }
    }
}
