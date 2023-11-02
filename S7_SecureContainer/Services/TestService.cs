using Blazored.Toast.Services;
using Docker.DotNet.Models;
using S7_SecureContainer.Models.Docker;
using S7_SecureContainer.Models.Test;
using System.ComponentModel;
using System.Diagnostics;

namespace S7_SecureContainer.Services
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
            List<ContainerListResponse> containers = await getContainerList(listParameters);

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

        private async Task<List<ContainerListResponse>> getContainerList(ContainersListParameters listParameters)
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

        private void RunTestsOnContainer(List<string> tests, ContainerListResponse container)
        {
            foreach (var testType in tests)
            {
                switch (testType)
                {
                    case ContainerTestTypes.CheckForRoot:
                        lock (Tasks)
                            Tasks.Add(Task.Run(() => CheckForRoot(container)));
                        break;
                    case ContainerTestTypes.CheckForDefaultNetwork:
                        lock (Tasks)
                            Tasks.Add(Task.Run(() => CheckForDefaultNetwork(container)));
                        break;
                    default:
                        break;
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

        private async Task CheckForDefaultNetwork(ContainerListResponse container)
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

        private async Task CheckLimitResources(ContainerListResponse container)
        {
            var config = await DockerService.Client.Containers.InspectContainerAsync(container.ID);
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

        private List<string> GetMissingTestResults(List<TestResult> testResults)
        {
            List<string> testTypes = new();
            foreach (var testResult in ContainerTestTypes.All){
                if (!testResults.Any(a => a.Message == testResult))
                {
                    testTypes.Add(testResult);
                }
            }
            return testTypes;
        }
    }
}
