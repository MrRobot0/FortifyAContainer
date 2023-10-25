using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic;
using S7_SecureContainer.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace S7_SecureContainer.Services
{
    public class DockerTest
    {
        private DockerService dockerService { get; set; }

        private Dictionary<ContainerListResponse, List<TestResult>> containerTestResults = new Dictionary<ContainerListResponse, List<TestResult>>();

        private List<Task> Tasks= new List<Task>();

        public async Task<Dictionary<ContainerListResponse, List<TestResult>>> TestContainers(ContainersListParameters listParameters, DockerService dockerService)
        {
            if (!Task.WhenAll(Tasks).IsCompleted)
            {
                throw new TestStillRunningExpection();
            }

            this.dockerService = dockerService;
            var containers = await dockerService.Client.Containers.ListContainersAsync(listParameters);
            
            containerTestResults.Clear();
            Tasks.Clear();

            foreach (ContainerListResponse container in containers)
            {
                containerTestResults.Add(container, new());
                Tasks.Add(Task.Run(() => CheckForRoot(container)));
                Tasks.Add(Task.Run(() => CheckForDefaultNetwork(container)));
            }

            await Task.WhenAll(Tasks);
            return containerTestResults;
        }

        private async Task CheckForRoot(ContainerListResponse container)
        {
            var list = containerTestResults[container];
            try
            {
                var config = await dockerService.Client.Containers.InspectContainerAsync(container.ID);
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
                var config = await dockerService.Client.Containers.InspectContainerAsync(container.ID);
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
