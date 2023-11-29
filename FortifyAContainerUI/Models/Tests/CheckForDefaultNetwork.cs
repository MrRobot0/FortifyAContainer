using Docker.DotNet.Models;
using FortifyAContainerUI.Models.Docker;
using FortifyAContainerUI.Models.Test;
using FortifyAContainerUI.Services;
using System.Collections.Generic;

namespace FortifyAContainerUI.Models.Tests
{
    public static class CheckForDefaultNetwork
    {
        public static async Task<TestResult> Run(ContainerInspectResponse containerInspectResponse)
        {
            try
            {
                var networks = containerInspectResponse.NetworkSettings.Networks;

                foreach (var network in networks)
                {
                    if (network.Key == "bridge")
                    {
                        return new TestResult(ContainerTestTypes.DefaultNetwork, TestResult.Status.Failed);
                    }
                }
                return new TestResult(ContainerTestTypes.DefaultNetwork, TestResult.Status.Passed);
            }
            catch (Exception)
            {
                return new TestResult(ContainerTestTypes.DefaultNetwork, TestResult.Status.Invalid);
            }
        }
    }
}
