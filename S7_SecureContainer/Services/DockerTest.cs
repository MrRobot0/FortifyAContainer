using Docker.DotNet.Models;
using S7_SecureContainer.Models;
using System.ComponentModel;

namespace S7_SecureContainer.Services
{
    public class DockerTest
    {
        private DockerService? DockerService { get; set; }

        private readonly Dictionary<ContainerListResponse, List<TestResult>> containerTestResults = new();

        private readonly List<Task> Tasks= new();

        public async Task<TestContainerModel> TestContainers(ContainersListParameters listParameters, DockerService dockerService)
        {
            if (!Task.WhenAll(Tasks).IsCompleted)
            {
                throw new TestStillRunningExpection();
            }

            DockerService = dockerService;
            var containers = await DockerService.Client.Containers.ListContainersAsync(listParameters);
            
            containerTestResults.Clear();
            Tasks.Clear();

            RunTests(containers);

            await Task.WhenAll(Tasks);
            ContainerFailedTests containerFailedTests = new();
            CheckIfTestAreComplete(containerTestResults, containerFailedTests);

            while (!containerFailedTests.TestComplete && containerFailedTests.RetryCount < 6)
            {
                ReRunTests(containerFailedTests);
                CheckIfTestAreComplete(containerTestResults, containerFailedTests);
                await Task.WhenAll(Tasks);
            }

            TestContainerModel testContainerModel = new();

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

        private void ReRunTests(ContainerFailedTests containerFailedTests)
        {
            foreach (var keyValuePair in containerFailedTests.ContainerTestResults)
            {
                var container = keyValuePair.Key;
                var failedTests = keyValuePair.Value;
                if (failedTests.Count == TestType.GetTestCount())
                {
                    containerTestResults.Remove(container);
                    containerTestResults.Add(container, new());
                }

                foreach (var testType in failedTests)
                {
                    switch (testType)
                    {
                        case TestType.CheckForRoot:
                            Tasks.Add(Task.Run(() => CheckForRoot(container)));
                            break;
                        case TestType.CheckForDefaultNetwork:
                            Tasks.Add(Task.Run(() => CheckForDefaultNetwork(container)));
                            break;
                        default:
                            break;
                    }
                }

            }
        }



        private void CheckIfTestAreComplete(Dictionary<ContainerListResponse, List<TestResult>> containerTestResults, 
            ContainerFailedTests containerFailedTests)
        {
            containerFailedTests.TestComplete = true;
            containerFailedTests.ContainerTestResults.Clear();
            containerFailedTests.RetryCount++;
            foreach (var containerTestResult in containerTestResults)
            {
                if (containerTestResult.Value.Count == TestType.GetTestCount())
                {
                    containerFailedTests.TestComplete = false;
                    containerFailedTests.ContainerTestResults.Add(
                        containerTestResult.Key, 
                        getMissingTestResults(containerTestResult.Value));
                }
            }
        }

        private List<string> getMissingTestResults(List<TestResult> testResults)
        {
            List<string> resultList = new();
            foreach (var testResult in TestType.All){
                if (!testResults.Any(a => a.Message == testResult))
                {
                    resultList.Add(testResult);
                }
            }
            return resultList;
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
            catch (Exception)
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
            catch (Exception)
            {
                list.Add(new TestResult(TestType.CheckForDefaultNetwork, TestResult.Status.Invalid, container));
            }
        }
    }
}
