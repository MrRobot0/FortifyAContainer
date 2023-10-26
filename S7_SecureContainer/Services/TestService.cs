using Docker.DotNet.Models;
using S7_SecureContainer.Models.Docker;
using S7_SecureContainer.Models.Test;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace S7_SecureContainer.Services
{
    public class TestService
    {
        private readonly List<Task> Tasks= new();
        private readonly Dictionary<ContainerListResponse, List<TestResult>> containerTestResults = new();

        public async Task<ContainerTestModel> TestDockerContainers(ContainersListParameters listParameters, DockerService dockerService)
        {
            if (!Task.WhenAll(Tasks).IsCompleted)
            {
                throw new TestStillRunningExpection();
            }

            ContainerFailedTests containerFailedTests = new();
            ContainerTestModel testContainerModel = new();
            ContainerTests containerTests = new(dockerService, containerTestResults);
            List<ContainerListResponse> containers = await containerTests.getContainerList(listParameters);

            containerTestResults.Clear();
            Tasks.Clear();

            RunTests(containers, containerTests);

            await Task.WhenAll(Tasks);
            CheckIfTestsAreComplete(containerTestResults, containerFailedTests);

            while (!containerFailedTests.TestComplete && containerFailedTests.RetryCount <= TestToastsMessages.RetryCount)
            {
                Tasks.Clear();
                ReRunTests(containerFailedTests, containerTests);
                await Task.WhenAll(Tasks);
                CheckIfTestsAreComplete(containerTestResults, containerFailedTests);
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

        private void RunTests(List<ContainerListResponse> containers, ContainerTests containerTests)
        {
            foreach (ContainerListResponse container in containers)
            {
                CleanContainerTestResult(container);
                RunTestsOnContainer(ContainerTestTypes.All, container, containerTests);
            }
        }

        private void ReRunTests(ContainerFailedTests containerFailedTests, ContainerTests containerTests)
        {
            foreach (var keyValuePair in containerFailedTests.ContainerTestResults)
            {
                var container = keyValuePair.Key;
                var failedTests = keyValuePair.Value;
                if (failedTests.Count == ContainerTestTypes.GetTestCount())
                {
                    CleanContainerTestResult(container);
                }
                RunTestsOnContainer(failedTests, container, containerTests);
            }
        }

        private void RunTestsOnContainer(List<string> tests, ContainerListResponse container, ContainerTests containerTests)
        {
            foreach (var testType in tests)
            {
                switch (testType)
                {
                    case ContainerTestTypes.CheckForRoot:
                        lock (Tasks)
                            Tasks.Add(Task.Run(() => containerTests.CheckForRoot(container)));
                        break;
                    case ContainerTestTypes.CheckForDefaultNetwork:
                        lock (Tasks)
                            Tasks.Add(Task.Run(() => containerTests.CheckForDefaultNetwork(container)));
                        break;
                    default:
                        break;
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
            var testComplete = true;
            containerFailedTests.ContainerTestResults.Clear();
            containerFailedTests.RetryCount++;
            foreach (var containerTestResult in containerTestResults)
            {
                if (containerTestResult.Value.Count == ContainerTestTypes.GetTestCount())
                {
                    testComplete = false;
                    containerFailedTests.ContainerTestResults.Add(
                        containerTestResult.Key, 
                        getMissingTestResults(containerTestResult.Value));
                }
            }
            containerFailedTests.TestComplete = testComplete;
        }

        private List<string> getMissingTestResults(List<TestResult> testResults)
        {
            List<string> resultList = new();
            foreach (var testResult in ContainerTestTypes.All){
                if (!testResults.Any(a => a.Message == testResult))
                {
                    resultList.Add(testResult);
                }
            }
            return resultList;
        }
    }
}
